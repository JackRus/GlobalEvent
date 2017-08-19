using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GlobalEvent.Data;
using GlobalEvent.Models.AdminViewModels;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using GlobalEvent.Models;

namespace GlobalEvent.Controllers
{
	[Authorize]
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
        [Authorize(Policy="Visitor Editor")]
        public async Task<IActionResult> Add(int? ID) // from Request/Index
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
        [Authorize(Policy="Visitor Editor")]
        public async Task<IActionResult> Add (Request r)
        {
            if (string.IsNullOrEmpty(r.Description) || r.VID != 0)
            {
                return RedirectToAction("Dashboard", "Admin");
            }

            ///    LOG     ///
            var user = await _userManager.GetUserAsync(User);
            await _db.Logs.AddAsync(user.CreateLog("Request", $"Request: \"{r.Description}\" for VID: {r.VID}, was CREATED"));
            /// END OF LOG ///


            await _db.SaveChangesAsync();
            return RedirectToAction("ViewVisitor", "Admin", new {ID = r.VID});
        }

        // [HttpGet]
        // [Authorize(Policy="Request EditorKiller")]
        // public async Task<IActionResult> Edit (int? ID)
        // {
        //     if (ID == null) 
        //     {
        //         return RedirectToAction("Index");
        //     }
        //     return View(await _db.Requests.FirstOrDefaultAsync(x => x.ID == ID));
        // }

        // [HttpPost]
        // [AutoValidateAntiforgeryToken]
        // [Authorize(Policy="Request EditorKiller")]
        // public async Task<IActionResult> Edit (Request t)
        // {
        //     if (!ModelState.IsValid)
        //     {
        //         return RedirectToAction("Index");
        //     }

        //     Request tOld = await _db.Requests.FirstOrDefaultAsync(x => x.ID == t.ID);
        //     tOld.Task = t.Task;
        //     tOld.Comments = t.Comments;
        //     tOld.Done = t.Done;
        //     tOld.EID = t.EID;
        //     tOld.Deadline = t.Deadline;
        //     _db.Requests.Update(tOld);

        //     ///    LOG     ///
        //     var user = await _userManager.GetUserAsync(User);
        //     await _db.Logs.AddAsync(user.CreateLog("Request", $"Task: {tOld.Task}, was EDITED"));
        //     /// END OF LOG ///

        //     await _db.SaveChangesAsync();
        //     return RedirectToAction("Index");
        // }

        // [HttpGet]
        // [Authorize(Policy="Request EditorKiller")]
        // public async Task<IActionResult> Delete (int? ID)
        // {
        //     if (ID == null) 
        //     {
        //         return RedirectToAction("Index");
        //     }

        //     Request t = await _db.Requests.FirstOrDefaultAsync(x => x.ID == ID);
        //     _db.Requests.Remove(t);

        //     ///    LOG     ///
        //     var user = await _userManager.GetUserAsync(User);
        //     await _db.Logs.AddAsync(user.CreateLog("Request", $"Task: {t.Task}, was DELETED"));
        //     /// END OF LOG ///

        //     await _db.SaveChangesAsync();
        //     return RedirectToAction("index");
        // }

        // [HttpGet]
        // [Authorize(Policy="Is Owner")]
        // public async Task<IActionResult> DeleteAll ()
        // {
        //     ViewBag.Requests = await _db.Requests.ToListAsync();
        //     return View();
        // }

        // [HttpGet]
        // [Authorize(Policy="Request EditorKiller")]
        // public async Task<IActionResult> Solved (int? ID)
        // {
        //     if (ID == null) 
        //     {
        //         return RedirectToAction("Index", "Request");
        //     }

        //     Request t = await _db.Requests.SingleOrDefaultAsync(x => x.ID == ID);
        //     t.Done = true;
        //     _db.Requests.Update(t);
        //     ///    LOG     ///
        //     var user = await _userManager.GetUserAsync(User);
        //     await _db.Logs.AddAsync(user.CreateLog("Request", $"Task: {t.Task}, was marked as DONE"));
        //     /// END OF LOG ///
        //     await _db.SaveChangesAsync();

        //     return RedirectToAction("Index", "Request");
        // }

        // [HttpGet]
        // [Authorize(Policy="Request EditorKiller")]
        // public async Task<IActionResult> Seen (int? ID)
        // {

        // }

        // [HttpGet]
        // [Authorize(Policy="Request EditorKiller")]
        // public async Task<IActionResult> Important (int? ID)
        // {
            
        // }
	}
}
