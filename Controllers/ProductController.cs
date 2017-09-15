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
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GlobalEvent.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private string _id;

		public ProductController (UserManager<ApplicationUser> userManager, ApplicationDbContext context, IHttpContextAccessor http)
        {
            _db = context;
            _userManager = userManager;
            _id = _userManager.GetUserId(http.HttpContext.User);
        }

        //
        // GET: Product/Index
        [HttpGet]
        [Authorize(Policy="Products Viewer")]
        public async Task<IActionResult> Index(int? ID) //displays all Products
        {
            if (ID == null) 
            {
                return RedirectToAction("Events", "Owner");
            }

            // get the event by id and all its products
            ViewBag.Event = await _db.Events.Include(x => x.Products).SingleOrDefaultAsync(x => x.ID == ID);

            return View();
        }

        //
        // GET: Product/Add
        [HttpGet]
        [Authorize(Policy="Product Creator")]
        public async Task<IActionResult> Add(int? ID)
        {
            if (ID == null) 
            {
                return RedirectToAction("Events", "Owner");
            }

            // get event by id
            ViewBag.Event = await _db.Events.SingleOrDefaultAsync(x => x.ID == ID);
            return View();
        }

        //
        // POST: Product/Add
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        [Authorize(Policy="Product Creator")]
        public async Task<IActionResult> Add(Product model)
        {
            if (ModelState.IsValid)
            {
                // add product
                Event currentEvent = await _db.Events.FirstOrDefaultAsync(x => x.ID == model.EID);
                
                // reset the ID 
                model.ID = 0; 
                currentEvent.Products.Add(model);
                _db.Events.Update(currentEvent);

                // log for admin
                await _db.Logs.AddAsync(await Log.New("Product", $"Product: {model.Name}, was CREATED", _id, _db));

                return RedirectToAction("Index", new { ID = model.EID });
            }
            return RedirectToAction("Events", "Owner");
        }

        //
        // GET: Product/Copy
        [HttpGet]
        [Authorize(Policy="Product Creator")]
        public async Task<IActionResult> Copy (int? ID)
        {
            if (ID == null) 
            {
                return RedirectToAction("Events", "Owner");
            }

            Product product = await _db.Products.SingleOrDefaultAsync(x => x.ID == ID);
            ViewBag.Event = (await _db.Events.SingleOrDefaultAsync(x => x.ID == product.EID)).Name;

            return View(product);
        }

        //
        // POST: Product/Copy
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        [Authorize(Policy="Product Creator")]
        public async Task<IActionResult> Copy (Product model)
        {
            if (ModelState.IsValid)
            {
                // update and save changes
                Event currentEvent = await _db.Events.FirstOrDefaultAsync(x => x.ID == model.EID);
                
                // reset the ID
                model.ID = 0;
                currentEvent.Products.Add(model);
                _db.Events.Update(currentEvent);
                
                // log for admin
                await _db.Logs.AddAsync(await Log.New("Product", $"Product: {model.Name}, was COPIED", _id, _db));

                return RedirectToAction("Index", new { ID = model.EID });
            }
            
            return RedirectToAction("Events", "Owner");
        }

        // 
        // GET: Product/Edit
        [HttpGet]
        [Authorize(Policy="Product Creator")]
        public async Task<IActionResult> Edit(int? ID)
        {
            if (ID == null) 
            {
                return RedirectToAction("Events", "Owner");
            }
            
            Product product = await _db.Products.FirstOrDefaultAsync(x => x.ID == ID);
            ViewBag.Event = (await _db.Events.SingleOrDefaultAsync(x => x.ID == product.EID)).Name;

            return View(product);
        }

        // 
        // POST: Product/Edit
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        [Authorize(Policy="Product Creator")]
        public async Task<IActionResult> Edit(Product model)
        {
            if (ModelState.IsValid)
            {
                // get and update product
                Product product = await _db.Products.SingleOrDefaultAsync(x => x.ID == model.ID);
                product.UpdateValues(model);
                _db.Products.Update(product);

                // log for admin
                await _db.Logs.AddAsync(await Log.New("Product", $"Product: {product.Name}, was EDITED", _id, _db));

                return RedirectToAction("Index", new { ID = model.EID });
            }

            return RedirectToAction("Events", "Owner");
        }

        //
        // GET: Product/Delete
        [HttpGet]
        [Authorize(Policy="Product Killer")]
        public async Task<IActionResult> Delete(int? ID)
        {
            if (ID == null) 
            {
                return RedirectToAction("Events", "Owner");
            }

            Product product = await _db.Products.FirstOrDefaultAsync(x => x.ID == ID);

            return View(product);
        }

        //
        // GET: Product/DeleteOk
        [HttpGet]
        [Authorize(Policy="Product Killer")]
        public async Task<IActionResult> DeleteOk(int? ID)
        {
            if (ID == null)
            {
                return RedirectToAction("Events", "Owner");
            }

            // find and delete product
            Product product = await _db.Products.FirstOrDefaultAsync(x => x.ID == ID);
            _db.Products.Remove(product);

            // log for admin
            await _db.Logs.AddAsync(await Log.New("Product", $"Product: {product.Name}, was DELETED", _id, _db));

            return RedirectToAction("Index", "Product", new { ID = product.EID });
        }

        //
        // GET: Product/DeleteAll
        [HttpGet]
        [Authorize(Policy="Is Owner")]
        public async Task<IActionResult> DeleteAll(int? ID)
        {
            if (ID == null) 
            {
                return RedirectToAction("Events", "Owner");
            }
            
            Event currentEvent = await _db.Events.Include(x => x.Products).FirstOrDefaultAsync(x => x.ID == ID);

            return View(currentEvent);
        }

        //
        // GET: Product/DeleteAllOk
        [HttpGet]
        [Authorize(Policy="Is Owner")]
        public async Task<IActionResult> DeleteAllOk(int? ID)
        {
            if (ID == null) 
            {
                return RedirectToAction("Events", "Owner");
            }

            // delete all products for specified event id
            _db.Products.RemoveRange(_db.Products.Where(x => x.EID == ID));
            
            // log for admin
            await _db.Logs.AddAsync(await Log.New("Product", $"All Products for Event ID: {ID}, were DELETED", _id, _db));

            return RedirectToAction("Index", new { ID = ID });
        }

        //
        // GET: Product/SelectTickets
        [HttpGet]
        [Authorize(Policy="Product Creator")]
        public async Task<IActionResult> SelectTickets (int? ID, int? EID) //product ID
        {
            if (ID == null || EID == null) 
            {
                return RedirectToAction("Events", "Owner");
            }
            
            // update tickets with Eventbrite
            await Ticket_Classes.UpdateEB(_db, (int)EID);

            // get all tickets for current event
            ViewBag.Tickets = await _db.Tickets.Where(x => x.EID == EID).OrderBy(x => x.Type).ToListAsync();

            // get current product
            ViewBag.Product = await _db.Products.SingleOrDefaultAsync(x => x.ID == ID);
            
            ViewBag.ID = (int)ID;
            ViewBag.EID = (int)EID;

            return View();
        }

        //
        // POST: Product/SelectTickets
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        [Authorize(Policy="Product Creator")]
        public async Task<IActionResult> SaveTickets (int? ID, string result) // product ID
        {
            if (ID == null) 
            {
                return RedirectToAction("Events", "Owner");
            }

            // find and update product's tickets
            Product p = await _db.Products.FirstOrDefaultAsync(x => x.ID == ID);
            p.TTypes = result;
            _db.Products.Update(p);

            // log for admin
            await _db.Logs.AddAsync(await Log.New("Product", $"Tickets for Product: {p.Name}, were UPDATED", _id, _db));
            
            return RedirectToAction("Index", new { ID = p.EID });
        }
    }
}