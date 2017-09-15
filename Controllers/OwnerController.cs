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

        //
        // GET: Owner/Index
        [HttpGet]
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

        //
        // GET: Owner/Dashboard
        [HttpGet]
        [Authorize(Policy="Owner's Dashboard")]
        public async Task <IActionResult> Dashboard ()
        {
            Event active = await _db.Events
                .Include(x => x.Tickets)
                .Include(x => x.Visitors)
                    .ThenInclude(x => x.Requests)
                .Include(x => x.Products)
                .FirstOrDefaultAsync(x => x.Status);
            
            if (active == null)
            {
                 return View();
            }

            // update tickets and orders data
            await Event.Update(_db, active.ID);
            ViewBag.CheckIned = active.Visitors.Where(x => x.CheckIned).Count();
            ViewBag.Registered = active.Visitors.Where(x => x.Registered).Count();
            
            // select all requests for current event
            ViewBag.Requests = new List<Request>();
            ViewBag.NotSeen = 0;
            ViewBag.Important = 0;
            foreach (var item in active.Visitors)
            {
                ViewBag.Requests.AddRange(item.Requests);
                ViewBag.NotSeen += item.Requests.Where(x => !x.SeenByAdmin).Count();
                ViewBag.Important += item.Requests.Where(x => x.Important).Count();
            }
            
            // get all blocked visitors
            ViewBag.Blocked = await _db.Visitors
                .Where(x => x.Blocked && x.EID == active.ID).ToListAsync();

            // get all deleted visitors
            ViewBag.Deleted = await _db.Visitors
                .Where(x => x.Deleted && x.EID == active.ID).ToListAsync();

            // total tickets amount
            ViewBag.AllTickets = 0;
            active.Tickets.ForEach(t => ViewBag.AllTickets += t.Limit);

            return View(active);
        }

        //
        // GET: Owner/Events
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

        //
        // GET: Owner/CreateEvent
        [HttpGet]
        [Authorize(Policy="Event Creator")]
        public IActionResult CreateEvent ()
        {
            return View();
        }

        //
        // POST: Owner/CreateEvent
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        [Authorize(Policy="Event Creator")]
        public async Task<IActionResult> CreateEvent (Event e)
        {
            if (ModelState.IsValid)
            {
                _db.Events.Add(e);
                
                // Log for admin
                await _db.Logs.AddAsync(await Log.New("Event", $"Event: {e.Name}, was created", _id, _db));

                return RedirectToAction("Events", new { message = "The Event was successfully created."});
            }

            return RedirectToAction("Events", new { message = "Event wasn't created. Something went wrong. Please try again."});
        }

        //
        // GET: Owner/EditEvent
        [HttpGet]
        [Authorize(Policy="Event Editor")]
        public async Task<IActionResult> EditEvent (int? ID)
        {
            if (ID == null) 
            {
                return RedirectToAction("Events");
            }

            Event edit = await _db.Events.SingleOrDefaultAsync(x => x.ID == ID);
            return View(edit);
        }

        //
        // POST: Owner/EditEvent
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        [Authorize(Policy="Event Editor")]
        public async Task<IActionResult> EditEvent (Event model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Events", new { message = "Something went wrong. Please try again."});
            }
            
            Event eventToEdit = await model.CopyValues(_db);
            
            // check if there is any active event in db
            if (!eventToEdit.Status 
                && model.Status 
                && await _db.Events.AnyAsync(x => x.Status))
            {
                eventToEdit.Status = false;
                ViewBag.Message = "One of the Events is currently ACTIVE. And only 1 event can be ACTIVE at a time. You have to change it's status in order to make any other event ACTIVE. All other changes were saved.";
            }
            else
            {
                // change status if no active event found
                eventToEdit.Status = model.Status;
            }
            
            _db.Events.Update(eventToEdit);        
            
            // log for admin
            await _db.Logs.AddAsync(await Log.New("Event", $"Event: {eventToEdit.Name}, was EDITED", _id, _db));

            return RedirectToAction("ViewEvent", new { ID = eventToEdit.ID, message = ViewBag.Message });
        }

        //
        // GET: Owner/ViewEvent
        [HttpGet]
        [Authorize(Policy="Event Viewer")]
		public async Task<IActionResult> ViewEvent(int? ID, string message = null)
		{
			if (ID == null) 
            {
                return RedirectToAction("Events");
            }

			// get event by ID
			Event eventToView = await _db.Events
				.Include(x => x.Tickets)
				.Include(x => x.Types)
				.Include(x => x.Products)
                .Include(x => x.Orders)
				.SingleOrDefaultAsync(x => x.ID == ID);
            
            // Update orders and tickets
            await Order.OrderUpdate(_db, eventToView.ID);
            await Event.Update(_db, eventToView.ID);

            // update attendance
            ViewBag.CheckIned = eventToView.Visitors.Where(x => x.CheckIned).Count();
            ViewBag.Registered = eventToView.Visitors.Where(x => x.Registered).Count();
            
            // total amount of tickets
            ViewBag.AllTickets = 0;
            eventToView.Tickets.ForEach(t => ViewBag.AllTickets += t.Limit);

            ViewBag.Message = message;

			return View(eventToView);
		}

        //
        // GET: Owner/DeleteEvent
        [HttpGet]
        [Authorize(Policy="Event Killer")]
        public async Task<IActionResult> DeleteEvent (int? ID)
        {
            if (ID == null) 
            {
                return RedirectToAction("Events");
            }
            Event eventToDelete = await _db.Events.SingleOrDefaultAsync(x => x.ID == ID);

            return View(eventToDelete);
        }

        //
        // GET: Owner/DeleteEventOk
        [HttpGet]
        [Authorize(Policy="Event Killer")]
        public async Task<IActionResult> DeleteEventOk (int? ID)
        {
            if (ID == null) 
            {
                return RedirectToAction("Events");
            }

            Event eventToDelete = await _db.Events.FirstOrDefaultAsync(x => x.ID == ID);
            _db.Events.Remove(eventToDelete);
            
            // log for admin
            await _db.Logs.AddAsync(await Log.New("Event", $"Event: {eventToDelete.Name}, was DELETED", _id, _db));

            return RedirectToAction("Events");
        }

        //
        // GET: Owner/Admins
        [HttpGet]
        [Authorize(Policy="Admins Viewer")]
        public async Task<IActionResult> Admins ()
        {
            // get all users/admins
            ViewBag.Admins = await _db.Users.ToListAsync();
            
            return View();
        }

        //
        // GET: Owner/Attention
        [HttpGet]
        [Authorize(Policy = "Owner's Menu")]
        public async Task<IActionResult> Attention ()
        {
            // get all unsolved and unseen requests
            ViewBag.Requests = await _db.Requests
                .Where(x => !x.Solved && !x.SeenByAdmin)
                .OrderBy(x => x.Important)
                    .ThenBy(x => x.Date)
                    .ThenBy(x => x.Time)
                .ToListAsync();

            // get all unsolved and unassigned issues
            ViewBag.Issues = await _db.Issues
                .Where(x => !x.Solved && !x.Assigned)
                .OrderBy(x => x.ExpectedToBeSolved)
                    .ThenBy(x => x.Date)
                    .ThenBy(x => x.Time)
                .ToListAsync();

            // get all unseen notes
            ViewBag.Notes = await _db.Notes
                .Where(x => !x.SeenByAdmin)
                .OrderBy(x => x.Important)
                    .ThenBy(x => x.Date)
                    .ThenBy(x => x.Time)
                .ToListAsync();
  
            return View();
        }
	}
}
