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
using System;

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
            Event active = await _db.Events
                .Include(x => x.Tickets)
                .Include(x => x.Products)
                .Include(x => x.Visitors)
                    .ThenInclude(x => x.Requests)
                .FirstOrDefaultAsync(x => x.Status);
            
            ViewBag.Active = active == null ? false : true;
            
            if (ViewBag.Active)
            {
                await Event.Update(_db, active.ID);
                ViewBag.CheckIned = active.Visitors.Where(x => x.CheckIned).Count();
                ViewBag.Registered = active.Visitors.Where(x => x.Registered).Count();
                ViewBag.Requests = active.GetAllRequests();
                ViewBag.AllTickets = active.Tickets.Select(x => x.Limit).Sum();
            }
            ViewBag.Message = message;
            return View(active);
        }

        [HttpGet]
        [Authorize(Policy="Visitors Viewer")]
        public async Task<IActionResult> Search (string ID = null)
        {
            if (ID == null) 
            {
                return RedirectToAction("Dashboard", "Admin", new {message = "Search couldn't be performed."});
            }

            List<Visitor> visitors = await Visitor.Search(ID, _db);
            ViewBag.ID = ID;

            // log for user
            await _db.Logs.AddAsync(await Log.New("Search", $"Search value: {ID}", _id, _db));
                
            if (visitors == null || visitors.Count == 0)
            {
                ViewBag.Message = "No records were found. Please try different search creteria or make sure your input is correct.";
            }

            return View(visitors);
        }

        [HttpGet]
        [Authorize(Policy="Visitor Details")]
        public async Task <IActionResult> ViewVisitor (int? ID)
        {
            if (ID == null)
            {
                return RedirectToAction("Dashboard", "Admin", new {message = "Request couldn't be executed."});
            }

            Visitor visitor = await _db.Visitors
                .Include(x => x.Requests)
                .Include(x => x.Notes)
                .Include(x => x.Logs)
                    .ThenInclude(x => x.CurrentState)
                .SingleOrDefaultAsync(x => x.ID == ID);

            if (visitor == null)
            {
                return RedirectToAction("Dashboard", "Admin", new {message = "Visitor couldn't be found. Please, try again."});
            }   
            return View(visitor);
        } 


        [HttpGet]
        [Authorize(Policy="Visitor Editor")]
        public async Task <IActionResult> EditVisitor (int? ID)
        {
            if (ID == null)
            {
                return RedirectToAction("Dashboard", "Admin", new {message = "Couldn't access the visitor."});
            }
    
            EditVisitor model = new EditVisitor();
            await model.SetValues(_db, (int)ID);

            return View(model);
        }


        [HttpPost]
        [Authorize(Policy="Visitor Editor")]
        public async Task<IActionResult> EditVisitor (EditVisitor model)
        {
            if (ModelState.IsValid)
            {
                // get visitor by id
                Visitor visitor = await _db.Visitors.SingleOrDefaultAsync(x => x.ID == model.ID);
                ApplicationUser user = await _userManager.GetUserAsync(User); 

                // logs for visitor
                string action;
                if (!visitor.Blocked && model.Blocked)
                {
                    action = "BLOCKED";
                }
                else if (visitor.Blocked && !model.Blocked)
                {
                    action = "UNBLOCKED";
                }
                else
                {
                    action = "CHANGED";
                }
                visitor.AddLog("ADMIN", $"{action} BY {user.Level}: {user.FirstName} {user.LastName}", true);
                
                // update and save visitor
                JackLib.CopyValues(model, visitor);
                _db.Visitors.Update(visitor);
                
                // log for admin
                await _db.Logs.AddAsync(await Log.New("Visitor", $"Visitor witg ID: {visitor.ID}, was EDITED", _id, _db));

                return RedirectToAction("ViewVisitor", "Admin", new {ID = model.ID});
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
            Visitor visitor = await _db.Visitors.SingleOrDefaultAsync(x => x.ID == ID);

            return View(visitor);
        }

        [HttpGet]
        [Authorize(Policy="Visitor Killer")]
        public async Task <IActionResult> DeleteOk (int? ID)
        {
            if (ID == null)
            {
                return RedirectToAction("Dashboard", "Admin", new {message = "Couldn't execute this request."});
            }
            
            // update and save visitor
            Visitor visitor = await _db.Visitors.SingleOrDefaultAsync(x => x.ID == ID);
            visitor.Deleted = true;
            
            // change the number of checked-in tickets for this order number
            await Order.Decrement(visitor.OrderNumber, _db);

            // get admin
            ApplicationUser user = await _userManager.GetUserAsync(User);
            
            // visitor log
            visitor.AddLog("ADMIN", $"DELETED BY {user.Level}: {user.FirstName} {user.LastName}");
            _db.Visitors.Update(visitor);
            
            // admin log
            await _db.Logs.AddAsync(await Log.New("Visitor", $"Visitor(ID: {visitor.ID}) {visitor.Name} {visitor.Last} was DELETED.", user.Id, _db));

            return RedirectToAction("ViewVisitor", "Admin", new {ID = visitor.ID});
        }

        [HttpGet]
        [Authorize(Policy="Visitor Killer")]
        public async Task <IActionResult> Reinstate (int? ID)
        {
            if (ID == null)
            {
                return RedirectToAction("Dashboard", "Admin", new {message = "Couldn't execute this request."});
            }
            
            // update visitor's status
            Visitor visitor = await _db.Visitors.SingleOrDefaultAsync(x => x.ID == ID);
            visitor.Deleted = false;
            
            // check if all tickets for this order were used
            Order order = await _db.Orders.SingleOrDefaultAsync(x => x.Number.ToString() == visitor.OrderNumber);
            if (order.Full)
            {
                // log for admin
                await _db.Logs.AddAsync(await Log.New("Visitor", $"Attempt to REINSTATE Visitor(ID: {visitor.ID}) {visitor.Name} {visitor.Last} failed. Order is FULL", _id, _db));

                return RedirectToAction("Dashboard", "Admin", new {message = "This Visitor can NOT be reinstated. All tickets were used. Advise the visitor to purchase another ticket or ask Manager for assistance."});
            }

            // admin
            ApplicationUser user = await _userManager.GetUserAsync(User);

            // log for visitor
            visitor.AddLog("ADMIN", $"REINSTATED BY {user.Level}: {user.FirstName} {user.LastName}");
            _db.Visitors.Update(visitor);

            // change the number of checked-in tickets for this order number
            await Order.Increment(visitor.OrderNumber, _db);

            // log for admin
            await _db.Logs.AddAsync(await Log.New("Visitor", $"Visitor(ID: {visitor.ID}) {visitor.Name} {visitor.Last} was REINSTATED.", _id, _db));

            return RedirectToAction("ViewVisitor", "Admin", new {ID = visitor.ID});
        }
    }
}
