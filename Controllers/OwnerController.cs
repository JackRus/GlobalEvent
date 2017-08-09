using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GlobalEvent.Data;
using GlobalEvent.Models.OwnerViewModels;
using GlobalEvent.Models.VisitorViewModels;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace GlobalEvent.Controllers
{
	[Authorize]
    public class OwnerController : Controller
    {
		private readonly ApplicationDbContext _db;

		public OwnerController (ApplicationDbContext context)
        {
            _db = context;
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
				.Include(x => x.Types)
				.Include(x => x.Products)
                .Include(x => x.Orders)
                .Include(x => x.Visitors)
                .FirstOrDefaultAsync(x => x.Status);
            // TODO Event.Update();
            ViewBag.Active = e == null ? false : true;
            if (ViewBag.Active)
            {
                //ViewBag.OrdersValue = _db.Orders
                //    .Sum(x => x.EID == e.ID);           
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
	}
}
