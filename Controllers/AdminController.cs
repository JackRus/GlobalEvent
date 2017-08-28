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
using Microsoft.AspNetCore.Http;

namespace GlobalEvent.Controllers
{
	[Authorize]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private string _id;

		public AdminController (UserManager<ApplicationUser> userManager, ApplicationDbContext context, IHttpContextAccessor http)
        {
            _db = context;
            _userManager = userManager;
            _id = _userManager.GetUserId(http.HttpContext.User);
        }

        [HttpGet]
        [Authorize(Policy="Visitors Viewer")]
        public async Task<IActionResult> Dashboard (string message = null)
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
                await Event.Update(_db, e.ID);
                ViewBag.CheckIned = e.Visitors.Where(x => x.CheckIned).Count();
                ViewBag.Registered = e.Visitors.Where(x => x.Registered).Count();
                ViewBag.Requests = e.GetAllRequests();
                ViewBag.AllTickets = e.Tickets.Select(x => x.Limit).Sum();
            }
            ViewBag.Message = message;
            return View(e);
        }

        [HttpGet]
        [Authorize(Policy="Visitors Viewer")]
        public async Task<IActionResult> Search (string ID = null)
        {
            if (ID == null) 
            {
                return RedirectToAction("Dashboard", "Admin", new {message = "Search couldn't be performed."});
            }

            List<Visitor> v = await Visitor.Search(ID, _db);
            await _db.Logs.AddAsync(await Log.New("Search", $"Search value: {ID}", _id, _db));
            ViewBag.ID = ID;
            if (v == null || v.Count == 0)
            {
                ViewBag.Message = "No records were found. Please try different search creteria or make sure your input is correct.";
            }

            return View(v);
        }

        [HttpGet]
        [Authorize(Policy="Visitor Details")]
        public async Task <IActionResult> ViewVisitor (int? ID)
        {
            if (ID == null)
            {
                return RedirectToAction("Dashboard", "Admin", new {message = "Request couldn't be executed."});
            }

            Visitor v = await _db.Visitors
                .Include(x => x.Requests)
                .Include(x => x.Notes)
                .Include(x => x.Logs)
                    .ThenInclude(x => x.CurrentState)
                .SingleOrDefaultAsync(x => x.ID == ID);

            if (v == null)
            {
                return RedirectToAction("Dashboard", "Admin", new {message = "Visitor couldn't be found. Please, try again."});
            }   
            return View(v);
        } 


        [HttpGet]
        [Authorize(Policy="Visitor Editor")]
        public async Task <IActionResult> EditVisitor (int? ID)
        {
            if (ID == null)
            {
                return RedirectToAction("Dashboard", "Admin", new {message = "Couldn't access the visitor."});
            }
    
            EditVisitor ev = new EditVisitor();
            await ev.SetValues(_db, (int)ID);
            return View(ev);
        }


        [HttpPost]
        [Authorize(Policy="Visitor Editor")]
        public async Task<IActionResult> EditVisitor (EditVisitor ev)
        {
            if (ModelState.IsValid)
            {
                var v = await _db.Visitors.SingleOrDefaultAsync(x => x.ID == ev.ID);
                var u = await _userManager.GetUserAsync(User); 

                if (!v.Blocked && ev.Blocked)
                {
                    v.AddLog("ADMIN", $"BLOCKED BY {u.Level}: {u.FirstName} {u.LastName}");
                }
                else if (v.Blocked && !ev.Blocked)
                {
                    v.AddLog("ADMIN", $"UNBLOCKED BY {u.Level}: {u.FirstName} {u.LastName}");
                }

                JackLib.CopyValues(ev, v);
                _db.Visitors.Update(v);
                await _db.Logs.AddAsync(await Log.New("Visitor", $"Visitor witg ID: {v.ID}, was EDITED", _id, _db));
                return RedirectToAction("ViewVisitor", "Admin", new {ID = ev.ID});
            }
            return RedirectToAction("Dashboard", "Admin", new {message = "Couldn't execute this request. Please try again."});
        }

        [HttpGet]
        [Authorize(Policy="Visitor Killer")]
        public async Task <IActionResult> DeleteVisitor (int? ID)
        {
            if (ID == null)
            {
                return RedirectToAction("Dashboard", "Admin", new {message = "Couldn't access the visitor."});
            }
    
            Visitor v = await _db.Visitors.SingleOrDefaultAsync(x => x.ID == ID);
            return View(v);
        }

        [HttpGet]
        [Authorize(Policy="Visitor Killer")]
        public async Task <IActionResult> DeleteOk (int? ID)
        {
            if (ID == null)
            {
                return RedirectToAction("Dashboard", "Admin", new {message = "Couldn't execute this request."});
            }
            
            Visitor v = await _db.Visitors.SingleOrDefaultAsync(x => x.ID == ID);
            v.Deleted = true;
            var u = await _userManager.GetUserAsync(User);
            v.AddLog("ADMIN", $"DELETED BY {u.Level}: {u.FirstName} {u.LastName}");
            _db.Visitors.Update(v);
            await _db.Logs.AddAsync(await Log.New("Visitor", $"Visitor(ID: {v.ID}) {v.Name} {v.Last} was DELETED.", _id, _db));
            await Order.Decrement(v.OrderNumber, _db);

            return RedirectToAction("ViewVisitor", "Admin", new {ID = v.ID});
        }

        [HttpGet]
        [Authorize(Policy="Visitor Killer")]
        public async Task <IActionResult> Reinstate (int? ID)
        {
            if (ID == null)
            {
                return RedirectToAction("Dashboard", "Admin", new {message = "Couldn't execute this request."});
            }
            
            Visitor v = await _db.Visitors.SingleOrDefaultAsync(x => x.ID == ID);
            v.Deleted = false;
            if ((await _db.Orders.SingleOrDefaultAsync(x => x.Number.ToString() == v.OrderNumber)).Full)
            {
                await _db.Logs.AddAsync(await Log.New("Visitor", $"Attempt to REINSTATE Visitor(ID: {v.ID}) {v.Name} {v.Last} failed.", _id, _db));

                return RedirectToAction("Dashboard", "Admin", new {message = "This Visitor can NOT be reinstated. All tickets were used. Advise the visitor to purchase another ticket or ask Manager for assistance."});
            }

            // log for visitor
            var user = await _userManager.GetUserAsync(User);
            v.AddLog("ADMIN", $"REINSTATED BY {user.Level}: {user.FirstName} {user.LastName}");
            _db.Visitors.Update(v);
            await Order.Increment(v.OrderNumber, _db);
            await _db.Logs.AddAsync(await Log.New("Visitor", $"Visitor(ID: {v.ID}) {v.Name} {v.Last} was REINSTATED.", _id, _db));

            return RedirectToAction("ViewVisitor", "Admin", new {ID = v.ID});
        }
    }
}
