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
        public IActionResult Index()
        {
            return View();
        }

        public async Task <IActionResult> Dashboard ()
        {
            return View(await _db.Events.ToListAsync());
        }

        public async Task<IActionResult> Events ()
        {
            return View(await _db.Events.ToListAsync());
        }
        
        // CREATE EVENT 

        [HttpGet]
        public IActionResult CreateEvent ()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateEvent (Event newEvent)
        {
            if (ModelState.IsValid)
            {
                _db.Events.Add(newEvent);
                _db.SaveChanges();
                var x = _db.Events.FirstOrDefault();
                return View("ViewEvent", x);
            }
            return View();
        }


        // EDIT EVENT

        [HttpGet]
        public async Task<IActionResult> EditEvent (int? ID)
        {
            // redirects if no event ID provided || direct access
            if (ID == null) return RedirectToAction("Events");
            return View(await _db.Events.FirstOrDefaultAsync(x => x.ID == ID));
        }

        [HttpPost]
        public async Task<IActionResult> EditEvent (Event newEvent)
        {
            if (ModelState.IsValid)
            {
                // extracting the event by ID
                Event eUpdate = await _db.Events.FirstOrDefaultAsync(e => e.ID == newEvent.ID);
                // TODO cut hte extra lines
                // updating the event's data
                eUpdate.Name = newEvent.Name;
                eUpdate.DateStart = newEvent.DateStart;
                eUpdate.DateEnd = newEvent.DateEnd;
                eUpdate.TimeStart = newEvent.TimeStart;
                eUpdate.TimeEnd = newEvent.TimeEnd;
                eUpdate.Free = newEvent.Free;
                eUpdate.RevPlan = newEvent.RevPlan;
                eUpdate.Status = newEvent.Status;
                // saving changes
                _db.Events.Update(eUpdate);
                // saving changes to a DB
                await _db.SaveChangesAsync();
                return RedirectToAction("ViewEvent", new { ID = eUpdate.ID });
            }
            return View("Events");
        }

		public async Task<IActionResult> ViewEvent(int? ID)
		{
			// redirects if no event ID provided || direct access
			if (ID == null)
				return RedirectToAction("Events");

			// extrats event with the matching ID
			Event e = await _db.Events
				.Include(x => x.Tickets)
				.Include(x => x.Types)
				.Include(x => x.Products)
				.FirstOrDefaultAsync(x => x.ID == ID);
			return View(e);
		}

        [HttpGet]
        public async Task<IActionResult> DeleteEvent (int? ID)
        {
            // redirects if no event ID provided || direct access
            if (ID == null)
                return RedirectToAction("Events");

            // extrats event with the matching ID
            Event e = await _db.Events.Where(x => x.ID == ID).FirstOrDefaultAsync();
            return View(e);
        }

        [HttpGet]
        public async Task<IActionResult> DeleteEventOk (int? ID)
        {
            // redirects if no event ID provided || direct access
            if (ID == null)
                return RedirectToAction("Events");

            // extrats event with the matching ID
            Event e = await _db.Events.FirstOrDefaultAsync(x => x.ID == ID);
            _db.Events.Remove(e);
            await _db.SaveChangesAsync();
            return RedirectToAction("Events");
        }
	}
}
