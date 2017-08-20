using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GlobalEvent.Data;
using GlobalEvent.Models.AdminViewModels;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using GlobalEvent.Models;
using GlobalEvent.Models.VisitorViewModels;

namespace GlobalEvent.Controllers
{
	[Authorize(Policy="Visitor Editor")]
    public class RequestController : Controller
    {
		private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

		public RequestController (UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _db = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Add(int? ID)
        {
            if (ID == null) 
            {
                return RedirectToAction("Dashboard", "Admin");
            }

            ViewBag.Visitor = await _db.Visitors.SingleOrDefaultAsync(x => x.ID == ID);
            return View(new Request());
        }
        
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Add (Request r)
        {
            if (string.IsNullOrEmpty(r.Description) || r.VID == 0)
            {
                return RedirectToAction("Dashboard", "Admin");
            }

            ///    LOG     ///
            var user = await _userManager.GetUserAsync(User);
            await _db.Logs.AddAsync(user.CreateLog("Request", $"Request: \"{r.Description}\" for VID: {r.VID}, was CREATED"));
            /// END OF LOG ///

            r.AdminID = user.Id;
            r.AdminName = $"{user.FirstName} {user.LastName}";

            // update visitor + add requests to db
            Visitor v = await _db.Visitors.SingleOrDefaultAsync(x => x.ID == r.VID);
            v.Requests.Add(r);
            _db.Visitors.Update(v);
            await _db.SaveChangesAsync();
            return RedirectToAction("ViewVisitor", "Admin", new {ID = r.VID});
        }

        [HttpGet]
        public async Task<IActionResult> Edit (int? ID)
        {
            if (ID == null) 
            {
                return RedirectToAction("Index");
            }
            ViewBag.Visitor = await _db.Visitors.SingleOrDefaultAsync(x => x.ID == ID);
            return View(await _db.Requests.FirstOrDefaultAsync(x => x.ID == ID));
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Edit (Request r)
        {
            if (string.IsNullOrEmpty(r.Description) || r.ID == 0)
            {
                return RedirectToAction("Dashboard", "Admin");
            }

            Request rOld = await _db.Requests.FirstOrDefaultAsync(x => x.ID == r.ID);
            rOld.Description = r.Description;
            rOld.SeenByAdmin = r.SeenByAdmin;
            rOld.Solved = r.Solved;
            rOld.Important = r.Important;
            _db.Requests.Update(rOld);

            ///    LOG     ///
            var user = await _userManager.GetUserAsync(User);
            await _db.Logs.AddAsync(user.CreateLog("Request", $"Request: \"{r.Description}\" for VID: {r.VID}, was EDITED"));
            /// END OF LOG ///

            await _db.SaveChangesAsync();
            return RedirectToAction("ViewVisitor", "Admin", new {ID = rOld.VID});
        }

        [HttpGet]
        public async Task<IActionResult> Delete (int? ID)
        {
            if (ID == null) 
            {
                return RedirectToAction("Dashboard", "Admin");
            }

            Request r = await _db.Requests.FirstOrDefaultAsync(x => x.ID == ID);
            _db.Requests.Remove(r);

            ///    LOG     ///
            var user = await _userManager.GetUserAsync(User);
            await _db.Logs.AddAsync(user.CreateLog("Request", $"Request: \"{r.Description}\" for VID: {r.VID}, was DELETED"));
            /// END OF LOG ///

            await _db.SaveChangesAsync();
            return RedirectToAction("ViewVisitor", "Admin", new {ID = r.VID});
        }

        [HttpGet]
        public async Task<IActionResult> Status (int? ID, string Name = null)
        {
            if (ID == null || Name == null)
            {
                return RedirectToAction("Dashboard", "Admin");
            }

            // get request by ID
            Request r = await _db.Requests.FirstOrDefaultAsync(x => x.ID == ID);
            
            switch (Name)
            {
                case "SEEN": 
                    r.SeenByAdmin = r.SeenByAdmin ? false : true;
                    break;
                case "IMPORTANT": 
                    r.Important = r.Important ? false : true;
                    break;
                case "SOLVED": 
                    r.Solved = r.Solved ? false : true;
                    break;
                default: 
                    break;
            }

            _db.Requests.Update(r);

            ///    LOG     ///
            var user = await _userManager.GetUserAsync(User);
            await _db.Logs.AddAsync(user.CreateLog("Request", $"Request: \"{r.Description}\" for VID: {r.VID}, feild {Name} was changed"));
            /// END OF LOG ///

            await _db.SaveChangesAsync();
            return RedirectToAction("ViewVisitor", "Admin", new {ID = r.VID});
        }
	}
}
