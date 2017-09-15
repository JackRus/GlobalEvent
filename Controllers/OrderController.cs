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

        //
        // GET: Order/AddOrder
        [HttpGet]
        [Authorize(Policy="Order Creator")]
        public async Task<IActionResult> AddOrder (int? ID = null)
        {
            if (ID == null)
            {
                return RedirectToAction("Dashboard", "Admin");
            }

            AddOrder order = new AddOrder();
            // assign event's ID
            order.EID = (int)ID;

            // lists for <select> for VType and Ticket type
            await order.CreateLists(_db);
        
            return View(order);
        }

        //
        // POST: Order/AddOrderOk
        [HttpPost]
        [Authorize(Policy="Order Creator")]
        public async Task<IActionResult> AddOrderOk (AddOrder model)
        {  
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Dashboard", "Admin");
            } 
        
            Order order = new Order();
            // check for duplicate and copy info
            bool hasDublicate = await model.CheckDuplicates(_db, order);

            if (!hasDublicate)
            {
                Event currentEvent = await _db.Events.SingleOrDefaultAsync(x => x.ID == model.EID);
                currentEvent.Orders.Add(order);
                _db.Events.Update(currentEvent);

                // log for admin
                await _db.Logs.AddAsync(await Log.New("Order", $"New Order: {order.ID}. Reason: {model.Comment} was CREATED", _id, _db));
            }
            else
            {
                ViewBag.Message = "This Order already exist.";
            }
            return View(order);
        } 

        //
        // GET: Order/AllOrders
        [HttpGet]
        [Authorize(Policy="Order Viewer")]
        public async Task<IActionResult> AllOrders (int? ID)
        {
            // get Active event id
            int EID = ID == null ? (await _db.Events.SingleOrDefaultAsync(x => x.Status)).ID : (int)ID;
 
            // all orders for this event id
            List<Order> orders = await _db.Orders.Where(x => x.EID == EID).ToListAsync(); 

            return View(orders);
        }

        //
        // GET: Order/EditOrder
        [HttpGet]
        [Authorize(Policy="Order Creator")]
        public async Task<IActionResult> EditOrder (int? ID = null)
        {
            if (ID == null)
            {
                return RedirectToAction("Dashboard", "Admin", new {message = "Couldn't execute this request. Please try again."});
            }
            Order order = await _db.Orders.SingleOrDefaultAsync(x => x.ID == ID);

            return View(order);
        }

        // 
        // POST: Order/EditOrder
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        [Authorize(Policy="Order Creator")]
        public async Task<IActionResult> EditOrder (Order model)
        {
            if (model == null)
            {
                return RedirectToAction("Dashboard", "Admin", new {message = "Couldn't execute this request. Please try again."});
            }

            // find and update values
            Order order = await _db.Orders.SingleOrDefaultAsync(x => x.ID == model.ID);
            order.CopyInfo(model);
            _db.Orders.Update(order);

            // log for admin
            await _db.Logs.AddAsync(await Log.New("Order", $"Order ID: {order.ID} was EDITED", _id, _db));

            return RedirectToAction("AllOrders", "Order", new {ID = order.EID});
        }


        // 
        // GET: Order/Cancel
        [HttpGet]
        [Authorize(Policy="Order Canceler")]
        public async Task<IActionResult> Cancel (int? ID = null)
        {
            if (ID == null)
            {
                return RedirectToAction("Dashboard", "Admin");
            }

            // find and update values
            Order order = await _db.Orders.SingleOrDefaultAsync(x => x.ID == ID);
            order.Cancelled = true;
            _db.Orders.Update(order);

            // log for admin
            await _db.Logs.AddAsync(await Log.New("Order", $"Order({order.ID}) #: {order.Number} was CANCELLED", _id, _db));

            return RedirectToAction("AllOrders", "Order", new {ID = order.EID});
        }
    }
}
