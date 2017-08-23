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
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

		public OrderController (ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _db = context;
            _userManager = userManager;
        }


        [HttpGet]
        public async Task<IActionResult> AddOrder (int? ID = null)
        {
            if (ID == null)
            {
                return RedirectToAction("Dashboard", "Admin");
            }

            AddOrder o = new AddOrder();
            o.EID = (int)ID;
            await o.CreateLists(_db);

            return View(o);
        }

        [HttpPost]
        public async Task<IActionResult> AddOrderOk (AddOrder o)
        {  
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Dashboard", "Admin");
            } 
        
            Order newO = new Order();
            if (!(await o.CheckDuplicates(_db, newO)))
            {
                // adding log
                var u = await _userManager.GetUserAsync(User);
                await _db.Logs.AddAsync(u.CreateLog("Order", $"New Order: {newO.ID}. Reason: {o.Comment}"));
                
                Event e = await _db.Events.SingleOrDefaultAsync(x => x.ID == o.EID);

                // update db
                e.Orders.Add(newO);
                _db.Events.Update(e);
                await _db.SaveChangesAsync();
            }
            else
            {
                ViewBag.Message = "This Order already exist.";
            }

            return View(newO);
        } 

        [HttpGet]
        public async Task<IActionResult> AllOrders (int? ID)
        {
            
            // get Active event id
            var EID = ID == null ? (await _db.Events.SingleOrDefaultAsync(x => x.Status)).ID : ID;
 
            var orders = await _db.Orders.Where(x => x.EID == EID).ToListAsync(); 
            return View(orders);
        }

        [HttpGet]
        public async Task<IActionResult> EditOrder (int? ID = null)
        {
            if (ID == null)
            {
                return RedirectToAction("Dashboard", "Admin");
            }

            Order o = await _db.Orders.SingleOrDefaultAsync(x => x.ID == ID);
            return View(o);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> EditOrder (Order o)
        {
            if (o == null)
            {
                return RedirectToAction("Dashboard", "Admin");
            }
            var oldO = await _db.Orders.SingleOrDefaultAsync(x => x.ID == o.ID);
            oldO.CopyInfo(o);

            _db.Orders.Update(oldO);

            // adding log
            var u = await _userManager.GetUserAsync(User);
            await _db.Logs.AddAsync(u.CreateLog("Order", $"Order ID: {oldO.ID} was EDITED"));

            await _db.SaveChangesAsync();
            
            return RedirectToAction("AllOrders", "Order", new {ID = oldO.EID});
        }


        [HttpGet]
        public async Task<IActionResult> Cancel (int? ID = null)
        {
            if (ID == null)
            {
                return RedirectToAction("Dashboard", "Admin");
            }

            Order o = await _db.Orders.SingleOrDefaultAsync(x => x.ID == ID);
            o.Cancelled = true;

            _db.Orders.Update(o);

            // adding log
            var u = await _userManager.GetUserAsync(User);
            await _db.Logs.AddAsync(u.CreateLog("Order", $"Order({o.ID}) #: {o.Number} was CANCELLED"));

            await _db.SaveChangesAsync();

            return RedirectToAction("AllOrders", "Order", new {ID = o.EID});
        }
    }
}
