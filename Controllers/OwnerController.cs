using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GlobalEvent.Data;
using GlobalEvent.Models.OwnerViewModels;
using GlobalEvent.Models.VisitorViewModels;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

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
            return View(await _db.Events.ToListAsync());
        }

        public async Task<IActionResult> Events ()
        {
            ViewBag.Active = await _db.Events.FirstOrDefaultAsync(x => x.Status);
            ViewBag.Future = await _db.Events.Where(x => !x.Status && !x.Archived).ToListAsync(); 
            ViewBag.Archived = await _db.Events.Where(x => x.Archived).ToListAsync();
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
                ViewBag.Message2 = "The Event was successfully created.";
                return View("Events", await _db.Events.ToListAsync());
            }
            ViewBag.Message = "Event wasn't created. Something went wrong. Please try again.";
            return View("Events", await _db.Events.ToListAsync());
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
                if (e.Status && await _db.Events.AnyAsync(x => x.Status))
                {
                    eOld.Status = false;
                    ViewBag.Message = "One of the Events is currently ACTIVE. And only 1 event can be ACTIVE at a time. You have to change it's status in order to make any other event ACTIVE.";
                }
                else eOld.Status = e.Status;

                // saving changes
                _db.Events.Update(eOld);
                await _db.SaveChangesAsync();
                return RedirectToAction("ViewEvent", new { ID = eOld.ID });
            }
            ViewBag.Message = "Something went wrong. Please try again.";
            return RedirectToAction("Events");
        }

        [HttpGet]
		public async Task<IActionResult> ViewEvent(int? ID)
		{
			if (ID == null) return RedirectToAction("Events");

			// extrats event with the matching ID
			Event e = await _db.Events
				.Include(x => x.Tickets)
				.Include(x => x.Types)
				.Include(x => x.Products)
                .Include(x => x.Orders)
				.FirstOrDefaultAsync(x => x.ID == ID);
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
