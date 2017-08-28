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

        [HttpGet]
        [Authorize(Policy="Products Viewer")]
        public async Task<IActionResult> Index(int? ID) //displays all Products
        {
            if (ID == null) 
            {
                return RedirectToAction("Events", "Owner");
            }

            // get all products for the current event
            ViewBag.Products = await _db.Products
                .Where(x => x.EID == ID)
                .ToListAsync();
            
            ViewBag.Event = (await _db.Events.SingleOrDefaultAsync(x => x.ID == ID)).Name;
            ViewBag.ID = ID;
            return View();
        }

        [HttpGet]
        [Authorize(Policy="Product Creator")]
        public async Task<IActionResult> Add(int? ID)
        {
            if (ID == null) 
            {
                return RedirectToAction("Events", "Owner");
            }

            ViewBag.Event = (await _db.Events.SingleOrDefaultAsync(x => x.ID == ID)).Name;
            return View(new Product(){EID = (int)ID});
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        [Authorize(Policy="Product Creator")]
        public async Task<IActionResult> Add(Product p)
        {
            // reset Product ID to 0
            p.ID = 0;
            if (ModelState.IsValid)
            {
                // update Event's products
                Event e = await _db.Events.FirstOrDefaultAsync(x => x.ID == p.EID);
                e.Products.Add(p);
                _db.Events.Update(e);

                await _db.Logs.AddAsync(await Log.New("Product", $"Product: {p.Name}, was CREATED", _id, _db));

                return RedirectToAction("Index", new { ID = p.EID });
            }
            return RedirectToAction("Events", "Owner");
        }

        [HttpGet]
        [Authorize(Policy="Product Creator")]
        public async Task<IActionResult> Copy (int? ID)
        {
            if (ID == null) 
            {
                return RedirectToAction("Events", "Owner");
            }

            Product p = await _db.Products.Where(x => x.ID == ID).FirstOrDefaultAsync();
            ViewBag.Event = (await _db.Events.SingleOrDefaultAsync(x => x.ID == ID)).Name;
            return View(p);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        [Authorize(Policy="Product Creator")]
        public async Task<IActionResult> Copy (Product p)
        {
            // reset Product ID to 0
            p.ID = 0;
            if (ModelState.IsValid)
            {
                // update and save changes
                Event e = await _db.Events.FirstOrDefaultAsync(x => x.ID == p.EID);
                e.Products.Add(p);
                _db.Events.Update(e);
                await _db.Logs.AddAsync(await Log.New("Product", $"Product: {p.Name}, was COPIED", _id, _db));

                return RedirectToAction("Index", new { ID = p.EID });
            }
            
            return RedirectToAction("Events", "Owner");
        }

        [HttpGet]
        [Authorize(Policy="Product Creator")]
        public async Task<IActionResult> Edit(int? ID)
        {
            if (ID == null) 
            {
                return RedirectToAction("Events", "Owner");
            }
            ViewBag.Event = (await _db.Events.SingleOrDefaultAsync(x => x.ID == ID)).Name;
            return View(await _db.Products.FirstOrDefaultAsync(x => x.ID == ID));
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        [Authorize(Policy="Product Creator")]
        public async Task<IActionResult> Edit(Product p)
        {
            if (ModelState.IsValid)
            {
                _db.Products.Update(p);
                await _db.Logs.AddAsync(await Log.New("Product", $"Product: {p.Name}, was EDITED", _id, _db));

                return RedirectToAction("Index", new { ID = p.EID });
            }
            return RedirectToAction("Events", "Owner");
        }

        [HttpGet]
        [Authorize(Policy="Product Killer")]
        public async Task<IActionResult> Delete(int? ID)
        {
            if (ID == null) 
            {
                return RedirectToAction("Events", "Owner");
            }

            return View(await _db.Products.FirstOrDefaultAsync(x => x.ID == ID));
        }

        [HttpGet]
        [Authorize(Policy="Product Killer")]
        public async Task<IActionResult> DeleteOk(int? ID)
        {
            if (ID == null)
            {
                return RedirectToAction("Events", "Owner");
            }

            Product p = await _db.Products.FirstOrDefaultAsync(x => x.ID == ID);
            _db.Products.Remove(p);
            await _db.Logs.AddAsync(await Log.New("Product", $"Product: {p.Name}, was DELETED", _id, _db));

            return RedirectToAction("Index", new { ID = p.EID });
        }

        [HttpGet]
        [Authorize(Policy="Is Owner")]
        public async Task<IActionResult> DeleteAll(int? ID)
        {
            if (ID == null) 
            {
                return RedirectToAction("Events", "Owner");
            }

            Event e = await _db.Events.Include(x => x.Products).FirstOrDefaultAsync(x => x.ID == ID);

            return View(e);
        }

        [HttpGet]
        [Authorize(Policy="Is Owner")]
        public async Task<IActionResult> DeleteAllOk(int? ID)
        {
            if (ID == null) 
            {
                return RedirectToAction("Events", "Owner");
            }

            _db.Products.RemoveRange(_db.Products.Where(x => x.EID == ID));
            await _db.Logs.AddAsync(await Log.New("Product", $"All Products for Event ID: {ID}, were DELETED", _id, _db));

            return RedirectToAction("Index", new { ID = ID });
        }

        [HttpGet]
        [Authorize(Policy="Product Creator")]
        public async Task<IActionResult> SelectTickets (int? ID, int? EID) //product ID
        {
            if (ID == null || EID == null) 
            {
                return RedirectToAction("Events", "Owner");
            }
            
            await Ticket_Classes.UpdateEB(_db, (int)EID);
            ViewBag.Tickets = await _db.Tickets.Where(x => x.EID == EID).OrderBy(x => x.Type).ToListAsync();
            ViewBag.Product = await _db.Products.FirstOrDefaultAsync(x => x.ID == ID);
            ViewBag.ID = (int)ID;
            ViewBag.EID = (int)EID;

            return View();
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        [Authorize(Policy="Product Creator")]
        public async Task<IActionResult> SaveTickets (int? ID, string result) // product ID
        {
            if (ID == null) 
            {
                return RedirectToAction("Events", "Owner");
            }

            Product p = await _db.Products.FirstOrDefaultAsync(x => x.ID == ID);
            p.TTypes = result;
            _db.Products.Update(p);
            await _db.Logs.AddAsync(await Log.New("Product", $"Tickets for Product: {p.Name}, were UPDATED", _id, _db));
            
            return RedirectToAction("Index", new { ID = p.EID });
        }
    }
}