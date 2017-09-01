using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GlobalEvent.Data;
using GlobalEvent.Models;
using GlobalEvent.Models.AdminViewModels;
using GlobalEvent.Models.EBViewModels;
using GlobalEvent.Models.OwnerViewModels;
using GlobalEvent.Models.VisitorViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GlobalEvent.Controllers
{
    [Authorize]
    public class TicketController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private string _id;

		public TicketController (UserManager<ApplicationUser> userManager, ApplicationDbContext context, IHttpContextAccessor http)
        {
            _db = context;
            _userManager = userManager;
            _id = _userManager.GetUserId(http.HttpContext.User);
        }

        [HttpGet]
        [Authorize(Policy="Tickets Viewer")]
        public async Task<IActionResult> Index(int? ID) //displays all ticket
        {
            if (ID == null) 
            {
                return RedirectToAction("Events", "Owner");
            }

            await Ticket_Classes.UpdateEB(_db, (int)ID);
            ViewBag.Event = await _db.Events.Include(x => x.Tickets).SingleOrDefaultAsync(x => x.ID == ID);

            return View();
        }

        [HttpGet]
        [Authorize(Policy="Ticket Creator")]
        public async Task<IActionResult> Add(int? ID)
        {
            if (ID == null) 
            {
                return RedirectToAction("Events", "Owner");
            }

            ViewBag.Event = await _db.Events.SingleOrDefaultAsync(x => x.ID == ID);
            return View();
        }


        [HttpPost]
        [AutoValidateAntiforgeryToken]
        [Authorize(Policy="Ticket Creator")]
        public async Task<IActionResult> Add(Ticket t)
        {
            if (ModelState.IsValid)
            {
                Event e = await _db.Events.FirstOrDefaultAsync(x => x.ID == t.EID);
                t.ID = 0; // reset the ID before adding to DB
                e.Tickets.Add(t);
                _db.Events.Update(e);
                await _db.Logs.AddAsync(await Log.New("Ticket", $"Ticket: {t.Type}, was CREATED", _id, _db));

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
            Ticket t = await _db.Tickets.FirstOrDefaultAsync(x => x.ID == ID);
            ViewBag.Event = (await _db.Events.SingleOrDefaultAsync(x => x.ID == t.EID)).Name;
            
            return View(t);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        [Authorize(Policy="Ticket Creator")]
        public async Task<IActionResult> Edit(Ticket t)
        {
            if (ModelState.IsValid)
            {
                _db.Tickets.Update(t);
                await _db.Logs.AddAsync(await Log.New("Ticket", $"Ticket: {t.Type}, was EDITED", _id, _db));

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
            Ticket t = await _db.Tickets.SingleOrDefaultAsync(x => x.ID == ID);
            ViewBag.Event = (await _db.Events.SingleOrDefaultAsync(x => x.ID == t.EID)).Name;
            return View(t);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        [Authorize(Policy="Ticket Creator")]
        public async Task<IActionResult> Copy (Ticket t)
        { 
            if (ModelState.IsValid)
            {
                Event e = await _db.Events.FirstOrDefaultAsync(x => x.ID == t.EID);
                t.ID = 0; // reset ID beofre adding to db
                e.Tickets.Add(t);
                _db.Events.Update(e);
                await _db.Logs.AddAsync(await Log.New("Ticket", $"Ticket: {t.Type}, was COPIED", _id, _db));

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

            Ticket t = await _db.Tickets.FirstOrDefaultAsync(x => x.ID == ID);
            _db.Tickets.Remove(t);
            await _db.Logs.AddAsync(await Log.New("Ticket", $"Ticket: {t.Type}, was DELETED", _id, _db));

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
            Event e = await _db.Events.Include(x => x.Tickets).FirstOrDefaultAsync(x => x.ID == ID);

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

            _db.Tickets.RemoveRange(_db.Tickets.Where(x => x.EID == ID));
            await _db.Logs.AddAsync(await Log.New("Ticket", $"All Tickets for Event ID: {ID}, were DELETED", _id, _db));

            return RedirectToAction("Index", new { ID = ID });
        }
    }
}