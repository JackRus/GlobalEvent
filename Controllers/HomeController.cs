using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GlobalEvent.Data;
using GlobalEvent.Models.OwnerViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GlobalEvent.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;

        public HomeController(ApplicationDbContext context)
        {
            _db = context;
        }
        
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Welcome()
        {
            ViewBag.Future = _db.Events.Where(x => !x.Status && !x.Archived);
            return View(await _db.Events.FirstOrDefaultAsync(x => x.Status));
        }

        public IActionResult Menu (int? EID)
        {
            if (EID == null) return RedirectToAction("Welcome", "Home");
            ViewBag.EID = (int)EID;
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
