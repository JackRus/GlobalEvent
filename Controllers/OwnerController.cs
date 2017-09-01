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
        public async Task<IActionResult> Index(string message = null)
        {
            // get all unfinished todos 
            ViewBag.Todos = _db.ToDos
                .Where(x => !x.Done)
                .OrderBy(x => x.Deadline)
                .ToList();
                
            ViewBag.Message = message;
            ViewBag.EventList = await ToDo.GenerateEventList(_db);
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

            ViewBag.Requests = await _db.Requests.OrderBy(x => x.ID).ToListAsync();
            ViewBag.Active = e == null ? false : true;
            
            if (ViewBag.Active)
            {
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
