using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GlobalEvent.Data;
using GlobalEvent.Models.OwnerViewModels;
using GlobalEvent.Models.VisitorViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GlobalEvent.Controllers
{
    [Authorize]
    public class TicketController : Controller
    {
        private readonly ApplicationDbContext _db;

        public TicketController(ApplicationDbContext context)
        {
            _db = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? ID) //displays all ticket for selected event
        {
            if (ID == null) return RedirectToAction("Events");

            // extrats event with the matching ID
            ViewBag.Tickets = await _db.Tickets.Where(x => x.EID == ID).ToListAsync();
            ViewBag.ID = ID;
            return View();
        }

        [HttpGet]
        public IActionResult Add(int? ID)
        {
            if (ID == null)return RedirectToAction("Events");
            Ticket t = new Ticket();
            t.EID = (int)ID;
            return View(t);
        }

        [HttpPost]
        public async Task<IActionResult> Add(Ticket t)
        {
            // reset ticket ID to 0
            t.ID = 0;
            if (ModelState.IsValid)
            {
                // extracting the event by ID
                Event e = await _db.Events.FirstOrDefaultAsync(x => x.ID == t.EID);
                e.Tickets.Add(t);
                // saving changes
                _db.Events.Update(e);
                // saving changes to a DB
                await _db.SaveChangesAsync();
                return RedirectToAction("Index", new { ID = t.EID });
            }
            return RedirectToAction("Events", "Owner");
        }

        // EDIT EVENT

        [HttpGet]
        public async Task<IActionResult> Edit(int? ID)
        {
            if (ID == null) return RedirectToAction("Events");
            return View(await _db.Tickets.FirstOrDefaultAsync(x => x.ID == ID));
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Ticket t)
        {
            if (ModelState.IsValid)
            {
                // saving changes
                _db.Tickets.Update(t);
                // saving changes to a DB
                await _db.SaveChangesAsync();
                return RedirectToAction("Index", new { ID = t.EID });
            }
            return RedirectToAction("Events", "Owner");
        }


        [HttpGet]
        public async Task<IActionResult> Copy (int? ID)
        {
            if (ID == null) return RedirectToAction("Events");
            return View(await _db.Tickets.FirstOrDefaultAsync(x => x.ID == ID));
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Copy (Ticket t)
        { 
            // reset VType ID to 0
            t.ID = 0;
            if (ModelState.IsValid)
            {
                // extracting the event by ID
                Event e = await _db.Events.FirstOrDefaultAsync(x => x.ID == t.EID);
                e.Tickets.Add(t);
                // saving changes
                _db.Events.Update(e);
                // saving changes to a DB
                await _db.SaveChangesAsync();
                return RedirectToAction("Index", new { ID = t.EID });
            }
            return RedirectToAction("Events", "Owner");
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? ID)
        {
            if (ID == null) return RedirectToAction("Events", "Owner");
            return View(await _db.Tickets.FirstOrDefaultAsync(x => x.ID == ID));
        }

        [HttpGet]
        public async Task<IActionResult> DeleteOk(int? ID)
        {
            if (ID == null)return RedirectToAction("Events");

            // extrats event with the matching ID
            Ticket t = await _db.Tickets.FirstOrDefaultAsync(x => x.ID == ID);
            _db.Tickets.Remove(t);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index", new { ID = t.EID });
        }

        [HttpGet]
        public async Task<IActionResult> DeleteAll(int? ID)
        {
            if (ID == null) return RedirectToAction("Events");

            // confirmation page
            Event e = await _db.Events
                .Include(x => x.Tickets)
                .FirstOrDefaultAsync(x => x.ID == ID);
            return View(e);
        }

        [HttpGet]
        public async Task<IActionResult> DeleteAllOk(int? ID)
        {
            if (ID == null) return RedirectToAction("Events");

            // delete all tickets for a specific event
            _db.Tickets.RemoveRange(_db.Tickets.Where(x => x.EID == ID));
            await _db.SaveChangesAsync();
            return RedirectToAction("Index", new { ID = ID });
        }
    }
}