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
        public async Task<IActionResult> Search (string ID = null, string Name = null)
        {
            if (ID == null || Name == null) return RedirectToAction("Dashboard");

            List<Visitor> v = new List<Visitor>();
            
            // get Active event id
            var EID = (await _db.Events.SingleOrDefaultAsync(x => x.Status)).ID;
            if (Name == "ID")
            {
                int intID;
                // convert to int
                int.TryParse(ID, out intID);
                // find all matches
                v = await _db.Visitors.Where(x => x.EID == EID && x.ID == intID).ToListAsync();
            }
            else if (Name == "Name")
            {

            }
            else if (Name == "Last")
            {

            }
            else if (Name == "Order")
            {

            }
            else if (Name == "RegNumber")
            {

            }
            else if (Name == "Email")
            {

            }

            return View(v);
        }

        public async Task <IActionResult> ListAll ()
        {
            // get Active event id
            var EID = (await _db.Events.SingleOrDefaultAsync(x => x.Status)).ID;
            List<Visitor> v = await _db.Visitors.Where(x => x.EID == EID).ToListAsync(); 
            return View(v);
        }

    }
}
