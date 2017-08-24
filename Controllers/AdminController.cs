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
using Microsoft.AspNetCore.Mvc.Rendering;

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

        [HttpGet]
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

            // ==> LOG
            var u = await _userManager.GetUserAsync(User);
            await _db.Logs.AddAsync(u.CreateLog("Search", $"Search value: {ID}"));
            // ==> END OF LOG
            await _db.SaveChangesAsync();
            ViewBag.Criteria = ID == "All" ? "All Visitors" : ID;

            if (v == null || v.Count == 0)
            {
                ViewBag.Message = "No records were found. Please try different search creteria or make sure your input is correct.";
            }
            return View(v);
        }

        [HttpGet]
        [Authorize(Policy="Visitors Viewer")]
        public async Task <IActionResult> ViewVisitor (int? ID)
        {
            if (ID == null)
            {
                return RedirectToAction("Dashboard", "Admin", new {message = "Request couldn't be executed."});
            }

            Visitor v = await _db.Visitors.SingleOrDefaultAsync(x => x.ID == ID);
            var d = JackLib.PropertiesTypes(v);
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
        public IActionResult EditVisitor (Visitor v)
        {
            // if (ModelState.IsValid)
            // {
                
            // }

            return RedirectToAction("ViewVisitor", "Admin");
        }

    }
}
