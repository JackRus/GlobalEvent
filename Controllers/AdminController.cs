using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GlobalEvent.Data;
using GlobalEvent.Models.OwnerViewModels;
using Microsoft.EntityFrameworkCore;
using GlobalEvent.Models.VisitorViewModels;
using Microsoft.AspNetCore.Identity;
using GlobalEvent.Models;
using GlobalEvent.Models.AdminViewModels;

namespace GlobalEvent.Controllers
{
	[Authorize]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

		public AdminController (ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _db = context;
            _userManager = userManager;
        }


        public async Task<IActionResult> Dashboard ()
        {
            Event e = await _db.Events
                .Include(x => x.Tickets)
                .Include(x => x.Products)
                .Include(x => x.Visitors)
                    .ThenInclude(x => x.Requests)
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

                // select all requests for current event
                ViewBag.Requests = new List<Request>();
                foreach (var item in e.Visitors)
                {
                    ViewBag.Requests.AddRange(item.Requests);
                }
                
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
        public async Task<IActionResult> Search (string ID = null)
        {
            if (ID == null) 
            {
                return RedirectToAction("Dashboard");
            }

            // get Active event id
            var EID = (await _db.Events.SingleOrDefaultAsync(x => x.Status)).ID;
           
            var parsed = 0;
            int.TryParse(ID, out parsed);
            List<Visitor> v = new List<Visitor>();
            if (ID == "All")
            {
                v = await _db.Visitors.Where(x => x.EID == EID).ToListAsync();
            }
            else
            {
                v = await _db.Visitors
                    .Where(x => x.EID == EID && (
                        x.ID == parsed ||
                        x.Name.ToUpper() == ID.ToUpper() ||
                        x.Last.ToUpper() == ID.ToUpper() ||
                        x.Email.ToUpper() == ID.ToUpper() ||
                        x.Company.ToUpper() == ID.ToUpper() ||
                        x.Occupation.ToUpper() == ID.ToUpper() ||
                        x.OrderNumber == ID ||
                        x.RegistrationNumber == ID
                    )).ToListAsync();
            }

            // adding log
            var u = await _userManager.GetUserAsync(User);
            await _db.Logs.AddAsync(u.CreateLog("Search", $"Search value: {ID}"));
            await _db.SaveChangesAsync();

            // passing search criteria 
            ViewBag.Criteria = ID;
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
                    .ThenInclude(x => x.CurrentState)
                .SingleOrDefaultAsync(x => x.ID == ID);
            
            return View(v);
        }
    }
}
