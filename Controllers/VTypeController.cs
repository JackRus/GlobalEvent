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
    public class VTypeController : Controller
    {
        private readonly ApplicationDbContext _db;

        public VTypeController(ApplicationDbContext context)
        {
            _db = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? ID) //displays all Products
        {
            if (ID == null) return RedirectToAction("Events");
            ViewBag.VTypes = await _db.Types
                .Where(x => x.EID == ID)
                .ToListAsync();
            ViewBag.ID = ID;
            return View();
        }

        [HttpGet]
        public IActionResult Add(int? ID)
        {
            if (ID == null) return RedirectToAction("Events");

            VType v = new VType();
            v.EID = (int)ID;
            return View(v);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Add(VType v)
        {
            // reset Product ID to 0
            v.ID = 0;
            if (ModelState.IsValid)
            {
                // extracting the event by ID
                Event e = await _db.Events.FirstOrDefaultAsync(x => x.ID == v.EID);
                e.Types.Add(v);
                // saving changes
                _db.Events.Update(e);
                // saving changes to a DB
                await _db.SaveChangesAsync();
                return RedirectToAction("Index", new { ID = v.EID });
            }
            return RedirectToAction("Events", "Owner");
        }

        [HttpGet]
        public async Task<IActionResult> Copy (int? ID)
        {
            if (ID == null) return RedirectToAction("Events");

            // extrats event with the matching ID
            VType v = await _db.Types
                .Where(x => x.ID == ID)
                .FirstOrDefaultAsync();
            return View(v);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Copy (VType v)
        { 
            // reset VType ID to 0 si
            v.ID = 0;
            if (ModelState.IsValid)
            {
                // extracting the event by ID
                Event e = await _db.Events.FirstOrDefaultAsync(x => x.ID == v.EID);
                e.Types.Add(v);
                // saving changes
                _db.Events.Update(e);
                // saving changes to a DB
                await _db.SaveChangesAsync();
                return RedirectToAction("Index", new { ID = v.EID });
            }
            return RedirectToAction("Events", "Owner");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? ID)
        {
            if (ID == null) return RedirectToAction("Events");
            return View(await _db.Types.FirstOrDefaultAsync(x => x.ID == ID));
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Edit(VType v)
        {
            if (ModelState.IsValid)
            {
                // updating the product
                _db.Types.Update(v);
                // saving changes to a DB
                await _db.SaveChangesAsync();
                return RedirectToAction("Index", new { ID = v.EID });
            }
            return RedirectToAction("Events", "Owner");
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? ID)
        {
            if (ID == null) return RedirectToAction("Events", "Owner");
            return View(await _db.Types.FirstOrDefaultAsync(x => x.ID == ID));
        }

        [HttpGet]
        public async Task<IActionResult> DeleteOk(int? ID)
        {
            if (ID == null)return RedirectToAction("Events");

            // extrats event with the matching ID
            VType v = await _db.Types.FirstOrDefaultAsync(x => x.ID == ID);
            _db.Types.Remove(v);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index", new { ID = v.EID });
        }

        [HttpGet]
        public async Task<IActionResult> DeleteAll(int? ID)
        {
            if (ID == null) return RedirectToAction("Events");

            // confirmation page
            Event e = await _db.Events
                .Include(x => x.Types)
                .FirstOrDefaultAsync(x => x.ID == ID);
            return View(e);
        }

        [HttpGet]
        public async Task<IActionResult> DeleteAllOk(int? ID)
        {
            // redirects if no event ID provided || direct access
            if (ID == null) return RedirectToAction("Events");

            // delete all Products for a specific event
            _db.Types.RemoveRange(_db.Types.Where(x => x.EID == ID));
            await _db.SaveChangesAsync();
            return RedirectToAction("Index", new { ID = ID });
        }
    }
}