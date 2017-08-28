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
using GlobalEvent.Models.AdminViewModels;
using Microsoft.AspNetCore.Http;

namespace GlobalEvent.Controllers
{
	[Authorize]
    public class OwnerController : Controller
    {
		private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private string _id;

		public OwnerController (UserManager<ApplicationUser> userManager, ApplicationDbContext context, IHttpContextAccessor http)
        {
            _db = context;
            _userManager = userManager;
            _id = _userManager.GetUserId(http.HttpContext.User);
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
        [Authorize(Policy="Owner's Dashboard")]
        public async Task <IActionResult> Dashboard ()
        {
            Event e = await _db.Events
                .Include(x => x.Tickets)
                .Include(x => x.Visitors)
                    .ThenInclude(x => x.Requests)
                .Include(x => x.Products)
                .FirstOrDefaultAsync(x => x.Status);

            ViewBag.Requests = await _db.Requests
                .OrderBy(x => x.ID)
                .ToListAsync();

            ViewBag.Active = e == null ? false : true;
            
            if (ViewBag.Active)
            {
                // Update orders
                await Order.OrderUpdate(_db, e.ID);
                await Event.Update(_db, e.ID);

                ViewBag.CheckIned = e.Visitors.Where(x => x.CheckIned).Count();
                ViewBag.Registered = e.Visitors.Where(x => x.Registered).Count();
                
                // select all requests for current event
                ViewBag.Requests = new List<Request>();
                ViewBag.NotSeen = 0;
                ViewBag.Important = 0;
                foreach (var item in e.Visitors)
                {
                    ViewBag.Requests.AddRange(item.Requests);
                    ViewBag.NotSeen += item.Requests.Where(x => !x.SeenByAdmin).Count();
                    ViewBag.Important += item.Requests.Where(x => x.Important).Count();
                }
                
                ViewBag.Blocked = await _db.Visitors
                    .Where(x => x.Blocked && x.EID == e.ID).ToListAsync();
                ViewBag.Deleted = await _db.Visitors
                    .Where(x => x.Deleted && x.EID == e.ID).ToListAsync();

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
                await _db.Logs.AddAsync(await Log.New("Event", $"Event: {e.Name}, was created", _id, _db));

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
            
            Event eOld = await e.CopyValues(_db);
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
            _db.Events.Update(eOld);        
            await _db.Logs.AddAsync(await Log.New("Event", $"Event: {eOld.Name}, was EDITED", _id, _db));

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
            
            // Update orders
            await Order.OrderUpdate(_db, e.ID);
            await Event.Update(_db, e.ID);
            ViewBag.CheckIned = e.Visitors.Where(x => x.CheckIned).Count();
            ViewBag.Registered = e.Visitors.Where(x => x.Registered).Count();
            ViewBag.AllTickets = 0;
            e.Tickets.ForEach(t => ViewBag.AllTickets += t.Limit);

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
            _db.Events.Remove(e);
            await _db.Logs.AddAsync(await Log.New("Event", $"Event: {e.Name}, was DELETED", _id, _db));

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
            EditAdmin ea = new EditAdmin();
            ea.FirstName = u.FirstName;
            ea.LastName = u.LastName;
            ea.Level = u.Level;
            ea.Email = u.Email;
            ea.Id = u.Id;

            // all user logs
            ViewBag.Logs = await _db.Logs.Where(x => x.AdminID == u.Id).OrderByDescending(x => x.ID).Take(100).ToListAsync();
            
            return View(ea);
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
            await _db.Logs.AddAsync(await Log.New("Edit User", $"{u.Level.ToUpper()}: {u.FirstName} {u.LastName}, was EDITED", _id, _db));
            await _db.Logs.AddAsync(await Log.New("Edit User", $"User was EDITED by {user.FirstName} {user.LastName}", _id, _db));

            // change password if new one is provided
            if (!string.IsNullOrEmpty(a.Password) 
                && !string.IsNullOrEmpty(a.ConfirmPassword) 
                && a.Password == a.ConfirmPassword)
            {
                await _userManager.RemovePasswordAsync(u);
                await _userManager.AddPasswordAsync(u, a.Password); 

                await _db.Logs.AddAsync(await Log.New("Edit User", $"{u.Level.ToUpper()}: {u.FirstName} {u.LastName}, PASSWORD was changed", _id, _db));
                await _db.Logs.AddAsync(await Log.New("Edit User", $"Password was changed by {user.FirstName} {user.LastName}", _id, _db));
            }

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
            ViewBag.Properties = properties.OrderBy(x => x.Name);
            ViewBag.AdminName = $"{u.Level}: {u.FirstName} {u.LastName}";
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
            await _db.Logs.AddAsync(await Log.New("Edit User", $"{u.Level.ToUpper()}: {u.FirstName} {u.LastName}, CLAIMS were changed", _id, _db));
            await _db.Logs.AddAsync(await Log.New("Edit User", $"User's CLAIMS were changed by {user.FirstName} {user.LastName}", _id, _db));

            return RedirectToAction("EditAdmin", "Owner", new {ID = ID});
        }

        [HttpGet]
        [Authorize(Policy = "Owner's Menu")]
        public async Task<IActionResult> Attention ()
        {
            ViewBag.Requests = await _db.Requests.Where(x => !x.Solved && !x.SeenByAdmin).OrderBy(x => x.Important).ThenBy(x => x.Date).ThenBy(x => x.Time).ToListAsync();

            ViewBag.Issues = await _db.Issues.Where(x => !x.Solved && !x.Assigned).OrderBy(x => x.ExpectedToBeSolved).ThenBy(x => x.Date).ThenBy(x => x.Time).ToListAsync();

            ViewBag.Notes = await _db.Notes.Where(x => !x.SeenByAdmin).OrderBy(x => x.Important).ThenBy(x => x.Date).ThenBy(x => x.Time).ToListAsync();
  
            return View();
        }
	}
}
