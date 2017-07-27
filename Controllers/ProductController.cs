using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GlobalEvent.Data;
using GlobalEvent.Models.OwnerViewModels;
using GlobalEvent.Models.VisitorViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GlobalEvent.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ProductController(ApplicationDbContext context)
        {
            _db = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? ID) //displays all Products
        {
            if (ID == null) return RedirectToAction("Events");
            ViewBag.Products = await _db.Products
                .Where(x => x.EID == ID)
                .ToListAsync();
            ViewBag.ID = ID;
            return View();
        }

        [HttpGet]
        public IActionResult Add(int? ID)
        {
            if (ID == null) return RedirectToAction("Events");

            Product p = new Product();
            p.EID = (int)ID;
            return View(p);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Add(Product newP)
        {
            // reset Product ID to 0
            newP.ID = 0;
            if (ModelState.IsValid)
            {
                // extracting the event by ID
                Event eUpdate = await _db.Events.FirstOrDefaultAsync(e => e.ID == newP.EID);
                eUpdate.Products.Add(newP);
                // saving changes
                _db.Events.Update(eUpdate);
                // saving changes to a DB
                await _db.SaveChangesAsync();
                return RedirectToAction("Index", new { ID = newP.EID });
            }
            return RedirectToAction("Events", "Owner");
        }

        [HttpGet]
        public async Task<IActionResult> Copy (int? ID)
        {
            if (ID == null) return RedirectToAction("Events");

            // extrats event with the matching ID
            Product p = await _db.Products
                .Where(x => x.ID == ID)
                .FirstOrDefaultAsync();
            return View(p);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Copy (Product newP)
        {
            // reset Product ID to 0 and store current value
            newP.ID = 0;

            if (ModelState.IsValid)
            {
                // extracting the event by ID
                Event eUpdate = await _db.Events.FirstOrDefaultAsync(e => e.ID == newP.EID);
                eUpdate.Products.Add(newP);
                // saving changes
                _db.Events.Update(eUpdate);
                // saving changes to a DB
                await _db.SaveChangesAsync();
                return RedirectToAction("Index", new { ID = newP.EID });
            }
            return RedirectToAction("Events", "Owner");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? ID)
        {
            if (ID == null) return RedirectToAction("Events");
            return View(await _db.Products.FirstOrDefaultAsync(x => x.ID == ID));
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Edit(Product newP)
        {
            if (ModelState.IsValid)
            {
                // updating the product
                _db.Products.Update(newP);
                // saving changes to a DB
                await _db.SaveChangesAsync();
                return RedirectToAction("Index", new { ID = newP.EID });
            }
            return RedirectToAction("Events", "Owner");
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? ID)
        {
            if (ID == null) return RedirectToAction("Events", "Owner");
            return View(await _db.Products.FirstOrDefaultAsync(x => x.ID == ID));
        }

        [HttpGet]
        public async Task<IActionResult> DeleteOk(int? ID)
        {
            if (ID == null)return RedirectToAction("Events");

            // extrats event with the matching ID
            Product p = await _db.Products.FirstOrDefaultAsync(x => x.ID == ID);
            _db.Products.Remove(p);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index", new { ID = p.EID });
        }

        [HttpGet]
        public async Task<IActionResult> DeleteAll(int? ID)
        {
            if (ID == null) return RedirectToAction("Events");

            // confirmation page
            Event e = await _db.Events
                .Include(x => x.Products)
                .FirstOrDefaultAsync(x => x.ID == ID);
            return View(e);
        }

        [HttpGet]
        public async Task<IActionResult> DeleteAllOk(int? ID)
        {
            // redirects if no event ID provided || direct access
            if (ID == null) return RedirectToAction("Events");

            // delete all Products for a specific event
            _db.Products.RemoveRange(_db.Products.Where(x => x.EID == ID));
            await _db.SaveChangesAsync();
            return RedirectToAction("Index", new { ID = ID });
        }

        [HttpGet]
        public async Task<IActionResult> SelectTickets (int? ID, int? EID) // product ID
        {
            if (ID == null || EID == null) return RedirectToAction("Events");
            ViewBag.Tickets = await _db.Tickets.Where(x => x.EID == EID).ToListAsync();
            ViewBag.Types = (await _db.Products.FirstOrDefaultAsync(x => x.ID == ID)).TTypes;
            ViewBag.ID = (int)ID;
            ViewBag.EID = (int)EID;
            return View();
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> SaveTickets (int? ID, string result) // product ID
        {
            if (ID == null) return RedirectToAction("Events");
            Product p = await _db.Products.FirstOrDefaultAsync(x => x.ID == ID);
            p.TTypes = result;
            // updating the product
            _db.Products.Update(p);
            // saving changes to a DB
            await _db.SaveChangesAsync();
            return RedirectToAction("Index", new { ID = p.EID });
        }
    }
}