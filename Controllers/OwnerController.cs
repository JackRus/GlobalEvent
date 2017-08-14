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
        public IActionResult Index(string message = null)
        {
            ViewBag.Todos = _db.ToDos
                .Where(x => !x.Done)
                .OrderBy(x => x.Deadline)
                .ToList();
            ViewBag.Message = message;
            return View();
        }

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

                // Visitors
                ViewBag.CheckIned = e.Visitors.Where(x => x.CheckIned).Count();
                ViewBag.Registered = e.Visitors.Where(x => x.Registered).Count();

                
                // All tickets
                ViewBag.AllTickets = 0;
                foreach (Ticket t in e.Tickets)
                {
                    ViewBag.AllTickets += t.Limit;
                }

            }
            
            return View(e);
        }

        public async Task<IActionResult> Events (string message = null)
        {
            ViewBag.Active = await _db.Events.FirstOrDefaultAsync(x => x.Status);
            ViewBag.Future = await _db.Events.Where(x => !x.Status && !x.Archived).ToListAsync(); 
            ViewBag.Archived = await _db.Events.Where(x => x.Archived).ToListAsync();
            ViewBag.Message = message;
            return View();
        }

        [HttpGet]
        public IActionResult CreateEvent ()
        {
            return View();
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> CreateEvent (Event e)
        {
            if (ModelState.IsValid)
            {
                _db.Events.Add(e);
                await _db.SaveChangesAsync();
                return RedirectToAction("Events", new { message = "The Event was successfully created."});
            }
            return RedirectToAction("Events", new { message = "Event wasn't created. Something went wrong. Please try again."});
        }

        [HttpGet]
        public async Task<IActionResult> EditEvent (int? ID)
        {
            if (ID == null) return RedirectToAction("Events");
            return View(await _db.Events.FirstOrDefaultAsync(x => x.ID == ID));
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> EditEvent (Event e)
        {
            if (ModelState.IsValid)
            {
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
                if (!eOld.Status && e.Status && await _db.Events.AnyAsync(x => x.Status))
                {
                    eOld.Status = false;
                    ViewBag.Message = "One of the Events is currently ACTIVE. And only 1 event can be ACTIVE at a time. You have to change it's status in order to make any other event ACTIVE. All other changes were saved.";
                }
                else
                    eOld.Status = e.Status;

                // saving changes
                _db.Events.Update(eOld);
                await _db.SaveChangesAsync();
                return RedirectToAction("ViewEvent", new { ID = eOld.ID, message = ViewBag.Message });
            }
            return RedirectToAction("Events", new { message = "Something went wrong. Please try again."});
        }

        [HttpGet]
		public async Task<IActionResult> ViewEvent(int? ID, string message = null)
		{
			if (ID == null) return RedirectToAction("Events");

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
        public async Task<IActionResult> DeleteEvent (int? ID)
        {
            if (ID == null) return RedirectToAction("Events");
            return View(await _db.Events.FirstOrDefaultAsync(x => x.ID == ID));
        }

        [HttpGet]
        public async Task<IActionResult> DeleteEventOk (int? ID)
        {
            if (ID == null) return RedirectToAction("Events");

            Event e = await _db.Events.FirstOrDefaultAsync(x => x.ID == ID);
            _db.Events.Remove(e);
            await _db.SaveChangesAsync();
            return RedirectToAction("Events");
        }

        [HttpGet]
        public async Task<IActionResult> Admins ()
        {
            ViewBag.Admins = await _db.Users.ToListAsync();
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> EditAdmin (string ID = null)
        {
            if (ID == null) RedirectToAction("Admins", "Owner");
            var u = await _userManager.FindByIdAsync(ID);
            ViewBag.Claims = await _userManager.GetClaimsAsync(u);
            EditAdmin a = new EditAdmin();
            a.FirstName = u.FirstName;
            a.LastName = u.LastName;
            a.Level = u.Level;
            a.Email = u.Email;
            a.Id = u.Id;
            return View(a);
        }

        [HttpPost]
        public async Task<IActionResult> EditAdmin (EditAdmin a)
        {
            if (!ModelState.IsValid) RedirectToAction("Admins", "Owner");
            var u = await _userManager.FindByIdAsync(a.Id);
            u.FirstName = a.FirstName;
            u.LastName = a.LastName;
            u.Level = a.Level;
            u.Email = a.Email;
            await _userManager.UpdateAsync(u);

            // change password if new one is provided
            if (!string.IsNullOrEmpty(a.Password) && !string.IsNullOrEmpty(a.ConfirmPassword) && a.Password == a.ConfirmPassword)
            {
                await _userManager.RemovePasswordAsync(u);
                await _userManager.AddPasswordAsync(u, a.Password); 
            }
            return RedirectToAction("Admins", "Owner");
        }

        [HttpGet]
        public async Task<IActionResult> ChangeClaims (string ID)
        {
            if (ID == null) RedirectToAction("Admins", "Owner");
            Claims c = new Claims();

            var properties = c.GetType().GetProperties().ToList();

            // existing claims
            var u = await _userManager.FindByIdAsync(ID);
            var hasClaims = await _userManager.GetClaimsAsync(u);

            // mapping
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
            ViewBag.AID = u.Id;
            return View(c);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeClaims (Claims c, string ID = null)
        {
            if (c == null || ID == null) RedirectToAction("Admins", "Owner");
            
            var u = await _userManager.FindByIdAsync(ID);
            var properties = c.GetType().GetProperties().ToList();
            
            // remove claims
            var hasClaims = await _userManager.GetClaimsAsync(u);
            await _userManager.RemoveClaimsAsync(u, hasClaims);
            
            // add claims
            foreach(var item in properties)
            {
                if ((bool)item.GetValue(c, null) == true)
                {
                    await _userManager.AddClaimAsync(u, new Claim(item.Name, ""));
                }
            }

            return RedirectToAction("EditAdmin", "Owner", new {ID = ID});
        }
	}
}
