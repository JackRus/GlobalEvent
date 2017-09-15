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
            // all future events
            ViewBag.Future = await _db.Events
                .Where(x => !x.Status && !x.Archived)
                .ToListAsync();
            
            // get active event
            ViewBag.Active = await _db.Events.FirstOrDefaultAsync(x => x.Status);
            return View();
        }

        [HttpGet]
        public IActionResult Menu (int? EID)
        {
            if (EID == null) 
            {
                return RedirectToAction("Welcome", "Home");
            }

            // event id
            ViewBag.EID = (int)EID;
            return View();
        }

        public IActionResult About()
        {
            return View();
        }
    }
}
