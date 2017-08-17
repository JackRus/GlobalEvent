using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GlobalEvent.Data;
using GlobalEvent.Models.OwnerViewModels;
using Microsoft.EntityFrameworkCore;
using GlobalEvent.Models.VisitorViewModels;

namespace GlobalEvent.Controllers
{
	[Authorize]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _db;

		public AdminController (ApplicationDbContext context)
        {
            _db = context;
        }


        public async Task<IActionResult> Dashboard ()
        {
            Event e = await _db.Events
                .Include(x => x.Tickets)
                .Include(x => x.Visitors)
                .FirstOrDefaultAsync(x => x.Status);

            // get active event
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

        [HttpGet]
        [Authorize(Policy="Visitors Viewer")]
        public async Task<IActionResult> Search (string Name = null, string ID = null)
        {
            if (ID == null || Name == null) 
            {
                return RedirectToAction("Dashboard");
            }

            List<Visitor> v = new List<Visitor>();
            
            // get Active event id
            var EID = (await _db.Events.SingleOrDefaultAsync(x => x.Status)).ID;
            if (Name == "ID")
            {
                // find all matches
                v = await _db.Visitors.Where(x => x.EID == EID && x.ID == int.Parse(ID)).ToListAsync();
            }
            else if (Name == "Name")
            {
                v = v = await _db.Visitors.Where(x => x.EID == EID && x.Name == ID).ToListAsync();
            }
            else if (Name == "Last")
            {
                v = v = await _db.Visitors.Where(x => x.EID == EID && x.Last == ID).ToListAsync();
            }
            else if (Name == "Order")
            {
                v = v = await _db.Visitors.Where(x => x.EID == EID && x.OrderNumber == ID).ToListAsync();
            }
            else if (Name == "RegNumber")
            {
                v = v = await _db.Visitors.Where(x => x.EID == EID && x.RegistrationNumber == ID).ToListAsync();
            }
            else if (Name == "Email")
            {
                v = v = await _db.Visitors.Where(x => x.EID == EID && x.Email == ID).ToListAsync();
            }

            if (v == null || v.Count == 0)
            {
                ViewBag.Message = "No Visitors were found. Please try different search creteria or make sure you input is correct.";
            }
            return View(v);
        }

        [HttpGet]
        [Authorize(Policy="Visitors Viewer")]
        public async Task <IActionResult> ListAll ()
        {
            // get Active event id
            var EID = (await _db.Events.SingleOrDefaultAsync(x => x.Status)).ID;
            
            // get all visitors for active event
            List<Visitor> v = await _db.Visitors.Where(x => x.EID == EID).ToListAsync(); 
            return View(v);
        }


        [HttpGet]
        [Authorize(Policy="Visitors Viewer")]
        public async Task <IActionResult> ViewVisitor (int? ID)
        {
            if (ID == null)
            {
                return RedirectToAction("Dashboard", "Admin");
            }

            Visitor v = await _db.Visitors
                .Include(x => x.Notes)
                .Include(x => x.Requests)
                .Include(x => x.Logs)
                .SingleOrDefaultAsync(x => x.ID == ID);
            
            return View(v);
        }


    }
}
