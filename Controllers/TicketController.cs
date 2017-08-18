using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GlobalEvent.Data;
using GlobalEvent.Models;
using GlobalEvent.Models.OwnerViewModels;
using GlobalEvent.Models.VisitorViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GlobalEvent.Controllers
{
    public class TicketController : Controller
    {
        private readonly ApplicationDbContext _db;
		private readonly UserManager<ApplicationUser> _userManager;

		public TicketController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _db = context;
            _userManager = userManager;
        }

        [HttpGet]
        [Authorize(Policy="Tickets Viewer")]
        public async Task<IActionResult> Index(int? ID) //displays all ticket
        {
            if (ID == null) 
            {
                return RedirectToAction("Events", "Owner");
            }

            // extrats event with the matching ID
            ViewBag.Tickets = await _db.Tickets
                .Where(x => x.EID == ID)
                .ToListAsync();

            ViewBag.ID = (int)ID;
            return View();
        }

        [HttpGet]
        [Authorize(Policy="Ticket Creator")]
        public IActionResult Add(int? ID)
        {
            if (ID == null) 
            {
                return RedirectToAction("Events", "Owner");
            }

            return View(new Ticket(){EID = (int)ID});
        }


        [HttpPost]
        [AutoValidateAntiforgeryToken]
        [Authorize(Policy="Ticket Creator")]
        public async Task<IActionResult> Add(Ticket t)
        {
            // reset ticket ID to 0
            t.ID = 0;
            if (ModelState.IsValid)
            {
                Event e = await _db.Events.FirstOrDefaultAsync(x => x.ID == t.EID);
                e.Tickets.Add(t);
                _db.Events.Update(e);

                var user = await _userManager.GetUserAsync(User);
                await _db.Logs.AddAsync(user.CreateLog("Ticket", $"Ticket: {t.Type}, was CREATED"));
                await _db.SaveChangesAsync();
                
                return RedirectToAction("Index", new { ID = t.EID });
            }
            return RedirectToAction("Events", "Owner");
        }

        [HttpGet]
        [Authorize(Policy="Ticket Creator")]
        public async Task<IActionResult> Edit(int? ID)
        {
            if (ID == null) 
            {
                return RedirectToAction("Events", "Owner");
            }

            return View(await _db.Tickets.FirstOrDefaultAsync(x => x.ID == ID));
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        [Authorize(Policy="Ticket Creator")]
        public async Task<IActionResult> Edit(Ticket t)
        {
            if (ModelState.IsValid)
            {
                _db.Tickets.Update(t);
                
                ///    LOG     ///
                var user = await _userManager.GetUserAsync(User);
                await _db.Logs.AddAsync(user.CreateLog("Ticket", $"Ticket: {t.Type}, was EDITED"));
                /// END OF LOG ///
                await _db.SaveChangesAsync();
                return RedirectToAction("Index", new { ID = t.EID });
            }
            return RedirectToAction("Events", "Owner");
        }

        [HttpGet]
        [Authorize(Policy="Ticket Creator")]
        public async Task<IActionResult> Copy (int? ID)
        {
            if (ID == null) 
            {
                return RedirectToAction("Events", "Owner");
            }

            return View(await _db.Tickets.FirstOrDefaultAsync(x => x.ID == ID));
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        [Authorize(Policy="Ticket Creator")]
        public async Task<IActionResult> Copy (Ticket t)
        { 
            // reset ticket ID to 0
            t.ID = 0;
            if (ModelState.IsValid)
            {
                // update and save changes
                Event e = await _db.Events.FirstOrDefaultAsync(x => x.ID == t.EID);
                e.Tickets.Add(t);
                _db.Events.Update(e);
                ///    LOG     ///
                var user = await _userManager.GetUserAsync(User);
                await _db.Logs.AddAsync(user.CreateLog("Ticket", $"Ticket: {t.Type}, was COPIED"));
                /// END OF LOG ///
                await _db.SaveChangesAsync();

                return RedirectToAction("Index", new { ID = t.EID });
            }
            return RedirectToAction("Events", "Owner");
        }

        [HttpGet]
        [Authorize(Policy="Ticket Killer")]
        public async Task<IActionResult> Delete(int? ID)
        {
            if (ID == null) 
            {
                return RedirectToAction("Events", "Owner");
            }

            return View(await _db.Tickets.FirstOrDefaultAsync(x => x.ID == ID));
        }

        [HttpGet]
        [Authorize(Policy="Ticket Killer")]
        public async Task<IActionResult> DeleteOk(int? ID)
        {
            if (ID == null) 
            {
                return RedirectToAction("Events", "Owner");
            }

            // extrats event with the matching ID
            Ticket t = await _db.Tickets.FirstOrDefaultAsync(x => x.ID == ID);
            _db.Tickets.Remove(t);
            ///    LOG     ///
            var user = await _userManager.GetUserAsync(User);
            await _db.Logs.AddAsync(user.CreateLog("Ticket", $"Ticket: {t.Type}, was DELETED"));
            /// END OF LOG ///
            await _db.SaveChangesAsync();

            return RedirectToAction("Index", new { ID = t.EID });
        }

        [HttpGet]
        [Authorize(Policy="Is Owner")]
        public async Task<IActionResult> DeleteAll(int? ID)
        {
            if (ID == null) 
            {
                return RedirectToAction("Events", "Owner");
            }

            // confirmation page
            Event e = await _db.Events
                .Include(x => x.Tickets)
                .FirstOrDefaultAsync(x => x.ID == ID);

            return View(e);
        }

        [HttpGet]
        [Authorize(Policy="Is Owner")]
        public async Task<IActionResult> DeleteAllOk(int? ID)
        {
            if (ID == null) 
            {
                return RedirectToAction("Events", "Owner");
            }

            // delete all tickets for a specific event and save changes
            _db.Tickets.RemoveRange(_db.Tickets.Where(x => x.EID == ID));
            ///    LOG     ///
            var user = await _userManager.GetUserAsync(User);
            await _db.Logs.AddAsync(user.CreateLog("Ticket", $"All Tickets for Event ID: {ID}, were DELETED"));
            /// END OF LOG ///
            await _db.SaveChangesAsync();

            return RedirectToAction("Index", new { ID = ID });
        }
    }
}