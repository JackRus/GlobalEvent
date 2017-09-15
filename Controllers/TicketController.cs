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

        //
        // GET: Ticket/Index
        [HttpGet]
        [Authorize(Policy="Tickets Viewer")]
        public async Task<IActionResult> Index(int? ID) //displays all ticket
        {
            if (ID == null) 
            {
                return RedirectToAction("Events", "Owner");
            }

            // update ticket types with Eventbrite
            await Ticket_Classes.UpdateEB(_db, (int)ID);

            // get the current event with all ticket types
            ViewBag.Event = await _db.Events.Include(x => x.Tickets).SingleOrDefaultAsync(x => x.ID == ID);

            return View();
        }

        //
        // GET: Ticket/Add
        [HttpGet]
        [Authorize(Policy="Ticket Creator")]
        public async Task<IActionResult> Add(int? ID)
        {
            if (ID == null) 
            {
                return RedirectToAction("Events", "Owner");
            }

            // get the current event
            ViewBag.Event = await _db.Events.SingleOrDefaultAsync(x => x.ID == ID);

            return View();
        }


        //
        // POST: Ticket/Add
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        [Authorize(Policy="Ticket Creator")]
        public async Task<IActionResult> Add(Ticket model)
        {
            if (ModelState.IsValid)
            {
                // reset the ID before adding to DB
                model.ID = 0;

                // get the current event and add ticket
                Event currentEvent = await _db.Events.FirstOrDefaultAsync(x => x.ID == model.EID);
                currentEvent.Tickets.Add(model);
                _db.Events.Update(currentEvent);

                // log for admin
                await _db.Logs.AddAsync(await Log.New("Ticket", $"Ticket: {model.Type}, was CREATED", _id, _db));

                return RedirectToAction("Index", new { ID = model.EID });
            }

            return RedirectToAction("Events", "Owner");
        }

        //
        // GET: Ticket/Edit
        [HttpGet]
        [Authorize(Policy="Ticket Creator")]
        public async Task<IActionResult> Edit(int? ID)
        {
            if (ID == null) 
            {
                return RedirectToAction("Events", "Owner");
            }
            
            // get the ticket to edit
            Ticket ticket = await _db.Tickets.FirstOrDefaultAsync(x => x.ID == ID);
            
            // get the event for the ticket
            ViewBag.Event = (await _db.Events.SingleOrDefaultAsync(x => x.ID == ticket.EID)).Name;
            
            return View(ticket);
        }

        //
        // POST: Ticket/Edit
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        [Authorize(Policy="Ticket Creator")]
        public async Task<IActionResult> Edit(Ticket model)
        {
            if (ModelState.IsValid)
            {
                // update ticket in db
                _db.Tickets.Update(model);

                // log for admin
                await _db.Logs.AddAsync(await Log.New("Ticket", $"Ticket: {model.Type}, was EDITED", _id, _db));

                return RedirectToAction("Index", new { ID = model.EID });
            }

            return RedirectToAction("Events", "Owner");
        }

        //
        // GET: Ticket/Copy
        [HttpGet]
        [Authorize(Policy="Ticket Creator")]
        public async Task<IActionResult> Copy (int? ID)
        {
            if (ID == null) 
            {
                return RedirectToAction("Events", "Owner");
            }
            
            Ticket ticket = await _db.Tickets.SingleOrDefaultAsync(x => x.ID == ID);
            JackLib.IfNull(ticket);

            // get current event's name
            ViewBag.Event = (await _db.Events.SingleOrDefaultAsync(x => x.ID == ticket.EID)).Name;

            return View(ticket);
        }

        // 
        // POST: Ticket/Copy
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        [Authorize(Policy="Ticket Creator")]
        public async Task<IActionResult> Copy (Ticket ticket)
        { 
            if (ModelState.IsValid)
            {
                Event eventToUpdate = await _db.Events.FirstOrDefaultAsync(x => x.ID == ticket.EID);

                // reset ID beofre adding to db
                ticket.ID = 0;
                eventToUpdate.Tickets.Add(ticket);
                _db.Events.Update(eventToUpdate);
                
                // log for admin
                await _db.Logs.AddAsync(await Log.New("Ticket", $"Ticket: {ticket.Type}, was COPIED", _id, _db));

                return RedirectToAction("Index", new { ID = ticket.EID });
            }

            return RedirectToAction("Events", "Owner");
        }

        //
        // GET: Ticket/Delete
        [HttpGet]
        [Authorize(Policy="Ticket Killer")]
        public async Task<IActionResult> Delete(int? ID)
        {
            if (ID == null) 
            {
                return RedirectToAction("Events", "Owner");
            }
            
            Ticket ticket = await _db.Tickets.FirstOrDefaultAsync(x => x.ID == ID);
            JackLib.IfNull(ticket);

            return View(ticket);
        }

        //
        // GET: Ticket/DeleteOk
        [HttpGet]
        [Authorize(Policy="Ticket Killer")]
        public async Task<IActionResult> DeleteOk(int? ID)
        {
            if (ID == null) 
            {
                return RedirectToAction("Events", "Owner");
            }

            Ticket ticket = await _db.Tickets.FirstOrDefaultAsync(x => x.ID == ID);
            _db.Tickets.Remove(ticket);

            // log for admin
            await _db.Logs.AddAsync(await Log.New("Ticket", $"Ticket: {ticket.Type}, was DELETED", _id, _db));

            return RedirectToAction("Index", new { ID = ticket.EID });
        }

        //
        // GET: Ticket/DeleteAll
        [HttpGet]
        [Authorize(Policy="Is Owner")]
        public async Task<IActionResult> DeleteAll(int? ID)
        {
            if (ID == null) 
            {
                return RedirectToAction("Events", "Owner");
            }
            
            Event eventToUpdate = await _db.Events.Include(x => x.Tickets).FirstOrDefaultAsync(x => x.ID == ID);

            return View(eventToUpdate);
        }

        // 
        // Ticket/DeleteAllOk
        [HttpGet]
        [Authorize(Policy="Is Owner")]
        public async Task<IActionResult> DeleteAllOk(int? ID)
        {
            if (ID == null) 
            {
                return RedirectToAction("Events", "Owner");
            }

            _db.Tickets.RemoveRange(_db.Tickets.Where(x => x.EID == ID));

            // log for admmin
            await _db.Logs.AddAsync(await Log.New("Ticket", $"All Tickets for Event ID: {ID}, were DELETED", _id, _db));

            return RedirectToAction("Index", new { ID = ID });
        }
    }
}