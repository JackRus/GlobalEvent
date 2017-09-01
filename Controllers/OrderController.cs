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
using Microsoft.AspNetCore.Http;

namespace GlobalEvent.Controllers
{
	[Authorize]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private string _id;

		public OrderController (UserManager<ApplicationUser> userManager, ApplicationDbContext context, IHttpContextAccessor http)
        {
            _db = context;
            _userManager = userManager;
            _id = _userManager.GetUserId(http.HttpContext.User);
        }

        [HttpGet]
        [Authorize(Policy="Order Creator")]
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
        [Authorize(Policy="Order Creator")]
        public async Task<IActionResult> AddOrderOk (AddOrder o)
        {  
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Dashboard", "Admin");
            } 
        
            Order newO = new Order();
            if (!(await o.CheckDuplicates(_db, newO)))
            {
                Event e = await _db.Events.SingleOrDefaultAsync(x => x.ID == o.EID);
                e.Orders.Add(newO);
                _db.Events.Update(e);
                await _db.Logs.AddAsync(await Log.New("Order", $"New Order: {newO.ID}. Reason: {o.Comment} was CREATED", _id, _db));
            }
            else
            {
                ViewBag.Message = "This Order already exist.";
            }
            return View(newO);
        } 

        [HttpGet]
        [Authorize(Policy="Order Viewer")]
        public async Task<IActionResult> AllOrders (int? ID)
        {
            // get Active event id
            int EID = ID == null ? (await _db.Events.SingleOrDefaultAsync(x => x.Status)).ID : (int)ID;
 
            List<Order> orders = await _db.Orders.Where(x => x.EID == EID).ToListAsync(); 
            return View(orders);
        }

        [HttpGet]
        [Authorize(Policy="Order Creator")]
        public async Task<IActionResult> EditOrder (int? ID = null)
        {
            if (ID == null)
            {
                return RedirectToAction("Dashboard", "Admin", new {message = "Couldn't execute this request. Please try again."});
            }
            return View(await _db.Orders.SingleOrDefaultAsync(x => x.ID == ID));
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        [Authorize(Policy="Order Creator")]
        public async Task<IActionResult> EditOrder (Order o)
        {
            if (o == null)
            {
                return RedirectToAction("Dashboard", "Admin", new {message = "Couldn't execute this request. Please try again."});
            }

            Order oldO = await _db.Orders.SingleOrDefaultAsync(x => x.ID == o.ID);
            oldO.CopyInfo(o);
            _db.Orders.Update(oldO);
            await _db.Logs.AddAsync(await Log.New("Order", $"Order ID: {oldO.ID} was EDITED", _id, _db));

            return RedirectToAction("AllOrders", "Order", new {ID = oldO.EID});
        }


        [HttpGet]
        [Authorize(Policy="Order Canceler")]
        public async Task<IActionResult> Cancel (int? ID = null)
        {
            if (ID == null)
            {
                return RedirectToAction("Dashboard", "Admin");
            }

            Order o = await _db.Orders.SingleOrDefaultAsync(x => x.ID == ID);
            o.Cancelled = true;
            _db.Orders.Update(o);
            await _db.Logs.AddAsync(await Log.New("Order", $"Order({o.ID}) #: {o.Number} was CANCELLED", _id, _db));

            return RedirectToAction("AllOrders", "Order", new {ID = o.EID});
        }
    }
}
