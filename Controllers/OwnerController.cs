using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GlobalEvent.Data;
using GlobalEvent.Models.OwnerViewModels;
using GlobalEvent.Models.VisitorViewModels;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using GlobalEvent.Models;
using System.Reflection;
using System.Security.Claims;

namespace GlobalEvent.Controllers
{
	[Authorize]
    public class OwnerController : Controller
    {
		private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

		public OwnerController (UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _db = context;
            _userManager = userManager;
        }

        [Authorize(Policy="Owner's Menu")]
        public IActionResult Index(string message = null)
        {
            // get all unfinished todos 
            ViewBag.Todos = _db.ToDos
                .Where(x => !x.Done)
                .OrderBy(x => x.Deadline)
                .ToList();
                
            ViewBag.Message = message;
            return View();
        }

        [HttpGet]
        [Authorize(Policy="Owner's Menu")]
        public async Task <IActionResult> Dashboard ()
        {
            Event e = await _db.Events
                .Include(x => x.Tickets)
                .Include(x => x.Visitors)
                .Include(x => x.Products)
                .FirstOrDefaultAsync(x => x.Status);

            ViewBag.Active = e == null ? false : true;
            
            if (ViewBag.Active)
            {
                // Update orders
                await Order.OrderUpdate(_db, e.ID);
                await Event.Update(_db, e.ID);

                // # of Visitors checkined
                ViewBag.CheckIned = e.Visitors.Where(x => x.CheckIned).Count();
                // # of visitors registered
                ViewBag.Registered = e.Visitors.Where(x => x.Registered).Count();
                
                // All tickets
                ViewBag.AllTickets = 0;
                e.Tickets.ForEach(t => ViewBag.AllTickets += t.Limit);
            }  
            return View(e);
        }

        [HttpGet]
        [Authorize(Policy="Events Viewer")]
        public async Task<IActionResult> Events (string message = null)
        {
            // get active event
            ViewBag.Active = await _db.Events.FirstOrDefaultAsync(x => x.Status);
            
            // get all upcoming events
            ViewBag.Future = await _db.Events
                .Where(x => !x.Status && !x.Archived)
                .ToListAsync(); 

            // get all archived events
            ViewBag.Archived = await _db.Events
                .Where(x => x.Archived)
                .ToListAsync();

            ViewBag.Message = message;
            return View();
        }

        [HttpGet]
        [Authorize(Policy="Event Creator")]
        public IActionResult CreateEvent ()
        {
            return View();
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        [Authorize(Policy="Event Creator")]
        public async Task<IActionResult> CreateEvent (Event e)
        {
            if (ModelState.IsValid)
            {
                _db.Events.Add(e);

                // log
                var user = await _userManager.GetUserAsync(User);
                await _db.Logs.AddAsync(user.CreateLog("Event", $"Event: {e.Name}, was created"));
                // save changes
                await _db.SaveChangesAsync();
                return RedirectToAction("Events", new { message = "The Event was successfully created."});
            }
            return RedirectToAction("Events", new { message = "Event wasn't created. Something went wrong. Please try again."});
        }

        [HttpGet]
        [Authorize(Policy="Event Editor")]
        public async Task<IActionResult> EditEvent (int? ID)
        {
            if (ID == null) 
            {
                return RedirectToAction("Events");
            }

            return View(await _db.Events.FirstOrDefaultAsync(x => x.ID == ID));
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        [Authorize(Policy="Event Editor")]
        public async Task<IActionResult> EditEvent (Event e)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Events", new { message = "Something went wrong. Please try again."});
            }
            
            Event eOld = await _db.Events.FirstOrDefaultAsync(x => x.ID == e.ID);
            
            // updating the event's data
            eOld.Name = e.Name;
            eOld.DateStart = e.DateStart;
            eOld.DateEnd = e.DateEnd;
            eOld.TimeStart = e.TimeStart;
            eOld.TimeEnd = e.TimeEnd;
            eOld.Free = e.Free;
            eOld.RevPlan = e.RevPlan;
            eOld.Archived = e.Archived;
            eOld.HttpBase = e.HttpBase;
            eOld.EventbriteID = e.EventbriteID;
            eOld.TicketLink = e.TicketLink;
            eOld.Description = e.Description; 
            
            if (!eOld.Status 
                && e.Status 
                && await _db.Events.AnyAsync(x => x.Status))
            {
                eOld.Status = false;
                ViewBag.Message = "One of the Events is currently ACTIVE. And only 1 event can be ACTIVE at a time. You have to change it's status in order to make any other event ACTIVE. All other changes were saved.";
            }
            else
            {
                eOld.Status = e.Status;
            }

            // saving changes
            _db.Events.Update(eOld);

            // log
            var user = await _userManager.GetUserAsync(User);
            await _db.Logs.AddAsync(user.CreateLog("Event", $"Event: {eOld.Name}, was EDITED"));

            await _db.SaveChangesAsync();

            return RedirectToAction("ViewEvent", new { ID = eOld.ID, message = ViewBag.Message });
        }

        [HttpGet]
        [Authorize(Policy="Event Viewer")]
		public async Task<IActionResult> ViewEvent(int? ID, string message = null)
		{
			if (ID == null) 
            {
                return RedirectToAction("Events");
            }

			// extrats event with the matching ID
			Event e = await _db.Events
				.Include(x => x.Tickets)
				.Include(x => x.Types)
				.Include(x => x.Products)
                .Include(x => x.Orders)
				.FirstOrDefaultAsync(x => x.ID == ID);
            
            ViewBag.Message = message;
			return View(e);
		}

        [HttpGet]
        [Authorize(Policy="Event Killer")]
        public async Task<IActionResult> DeleteEvent (int? ID)
        {
            if (ID == null) 
            {
                return RedirectToAction("Events");
            }

            return View(await _db.Events.FirstOrDefaultAsync(x => x.ID == ID));
        }

        [HttpGet]
        [Authorize(Policy="Event Killer")]
        public async Task<IActionResult> DeleteEventOk (int? ID)
        {
            if (ID == null) 
            {
                return RedirectToAction("Events");
            }

            Event e = await _db.Events.FirstOrDefaultAsync(x => x.ID == ID);
            
            // save changes
            _db.Events.Remove(e);

            // log
            var user = await _userManager.GetUserAsync(User);
            await _db.Logs.AddAsync(user.CreateLog("Event", $"Event: {e.Name}, was DELETED"));
            await _db.SaveChangesAsync();

            return RedirectToAction("Events");
        }

        [HttpGet]
        [Authorize(Policy="Admins Viewer")]
        public async Task<IActionResult> Admins ()
        {
            ViewBag.Admins = await _db.Users.ToListAsync();
            return View();
        }

        [HttpGet]
        [Authorize(Policy="Admin Editor")]
        public async Task<IActionResult> EditAdmin (string ID = null)
        {
            if (ID == null) 
            {
                return RedirectToAction("Admins", "Owner");
            }

            // get user from db by ID
            var u = await _userManager.FindByIdAsync(ID);
            ViewBag.Claims = await _userManager.GetClaimsAsync(u);
            
            // copy data to a view model
            EditAdmin a = new EditAdmin();
            a.FirstName = u.FirstName;
            a.LastName = u.LastName;
            a.Level = u.Level;
            a.Email = u.Email;
            a.Id = u.Id;
            
            return View(a);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        [Authorize(Policy="Admin Editor")]
        public async Task<IActionResult> EditAdmin (EditAdmin a)
        {
            if (!ModelState.IsValid) 
            {
                return RedirectToAction("Admins", "Owner");
            }

            // update user
            var u = await _userManager.FindByIdAsync(a.Id);
            u.FirstName = a.FirstName;
            u.LastName = a.LastName;
            u.Level = a.Level;
            u.Email = a.Email;
            await _userManager.UpdateAsync(u);

            // logs
            var user = await _userManager.GetUserAsync(User);
            await _db.Logs.AddAsync(user.CreateLog("Edit User", $"{u.Level.ToUpper()}: {u.FirstName} {u.LastName}, was EDITED"));
            await _db.Logs.AddAsync(u.CreateLog("Edit User", $"User was EDITED by {user.FirstName} {user.LastName}"));

            // change password if new one is provided
            if (!string.IsNullOrEmpty(a.Password) 
                && !string.IsNullOrEmpty(a.ConfirmPassword) 
                && a.Password == a.ConfirmPassword)
            {
                await _userManager.RemovePasswordAsync(u);
                await _userManager.AddPasswordAsync(u, a.Password); 

                // logs
                await _db.Logs.AddAsync(user.CreateLog("Edit User", $"{u.Level.ToUpper()}: {u.FirstName} {u.LastName}, PASSWORD was changed"));
                await _db.Logs.AddAsync(u.CreateLog("Edit User", $"Password was changed by {user.FirstName} {user.LastName}"));
            }
            await _db.SaveChangesAsync();
            return RedirectToAction("Admins", "Owner");
        }

        [HttpGet]
        [Authorize(Policy = "Claims Editor")]
        public async Task<IActionResult> ChangeClaims (string ID)
        {
            if (ID == null) 
            {
                return RedirectToAction("Admins", "Owner");
            }

            Claims c = new Claims();
            // get properties for Claims
            var properties = c.GetType().GetProperties().ToList();
            
            // get existing claims
            var u = await _userManager.FindByIdAsync(ID);
            var hasClaims = await _userManager.GetClaimsAsync(u);
            
            // if claim exist change the value to TRUE
            foreach (var claim in hasClaims)
            {
                foreach (var item in properties)
                {
                    if (item.Name == claim.Type)
                    {
                        item.SetValue(c, true);
                    }
                }
            }
            ViewBag.Properties = properties;
            
            // pass admin ID to a view
            ViewBag.AID = u.Id;
            
            return View(c);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        [Authorize(Policy = "Claims Editor")]
        public async Task<IActionResult> ChangeClaims (Claims c, string ID = null)
        {
            if (c == null || ID == null) 
            {
                return RedirectToAction("Admins", "Owner");
            }
            
            // get user by ID
            var u = await _userManager.FindByIdAsync(ID);
            var properties = c.GetType().GetProperties().ToList();
            
            // remove claims before assigning new ones
            var hasClaims = await _userManager.GetClaimsAsync(u);
            await _userManager.RemoveClaimsAsync(u, hasClaims);
            
            // add new claims
            foreach(var item in properties)
            {
                if ((bool)item.GetValue(c, null) == true)
                {
                    await _userManager.AddClaimAsync(u, new Claim(item.Name, ""));
                }
            }

            // logs
            var user = await _userManager.GetUserAsync(User);
            await _db.Logs.AddAsync(user.CreateLog("Edit User", $"{u.Level.ToUpper()}: {u.FirstName} {u.LastName}, CLAIMS were changed"));
            await _db.Logs.AddAsync(u.CreateLog("Edit User", $"User's CLAIMS were changed by {user.FirstName} {user.LastName}"));
            await _db.SaveChangesAsync();

            return RedirectToAction("EditAdmin", "Owner", new {ID = ID});
        }
	}
}
