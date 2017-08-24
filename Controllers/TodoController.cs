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
    public class TodoController : Controller
    {
		private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

		public TodoController (UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _db = context;
            _userManager = userManager;
        }

        [HttpGet]
        [Authorize(Policy="Todo Viewer")]
        public async Task<IActionResult> Index()
        {
            // get all active todos
            ViewBag.Todos = _db.ToDos
                .Where(x => !x.Done)
                .OrderBy(x => x.Deadline)
                .ToList();

            // get completed todos
            ViewBag.Done = await _db.ToDos
                .Where(x => x.Done)
                .OrderByDescending(x => x.Deadline)
                .Take(50)
                .ToListAsync();
            return View();
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        [Authorize(Policy="Todo Creator")]
        public async Task<IActionResult> Add (ToDo t, string url = "Owner") // from Main Menu
        {
            if (ModelState.IsValid)
            {
                _db.ToDos.Add(t);

                ///    LOG     ///
                var user = await _userManager.GetUserAsync(User);
                await _db.Logs.AddAsync(user.CreateLog("ToDo", $"Task: {t.Task}, was CREATED"));
                /// END OF LOG ///

                await _db.SaveChangesAsync();
                return RedirectToAction("Index", url);
            }
            return RedirectToAction("Index", "Owner", new { message = "Couldn't create ToDo item. Please try again."});
        }
        

        [HttpGet]
        [Authorize(Policy="Todo Creator")]
        public IActionResult AddFull() // from Todo/Index
        {
            return View(new ToDo());
        }

        [HttpGet]
        [Authorize(Policy="Todo Creator")]
        public async Task<IActionResult> Copy (int? ID)
        {
            if (ID == null) 
            {
                return RedirectToAction("Index");
            }

            return View(await _db.ToDos.FirstOrDefaultAsync(x => x.ID == ID));
        }

        [HttpGet]
        [Authorize(Policy="Todo EditorKiller")]
        public async Task<IActionResult> Edit (int? ID)
        {
            if (ID == null) 
            {
                return RedirectToAction("Index");
            }
            return View(await _db.ToDos.FirstOrDefaultAsync(x => x.ID == ID));
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        [Authorize(Policy="Todo EditorKiller")]
        public async Task<IActionResult> Edit (ToDo t)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Index");
            }

            ToDo tOld = await _db.ToDos.FirstOrDefaultAsync(x => x.ID == t.ID);
            tOld.Task = t.Task;
            tOld.Comments = t.Comments;
            tOld.Done = t.Done;
            tOld.EID = t.EID;
            tOld.Deadline = t.Deadline;
            _db.ToDos.Update(tOld);

            ///    LOG     ///
            var user = await _userManager.GetUserAsync(User);
            await _db.Logs.AddAsync(user.CreateLog("ToDo", $"Task: {tOld.Task}, was EDITED"));
            /// END OF LOG ///

            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Authorize(Policy="Todo EditorKiller")]
        public async Task<IActionResult> Delete (int? ID)
        {
            if (ID == null) 
            {
                return RedirectToAction("Index");
            }

            ToDo t = await _db.ToDos.FirstOrDefaultAsync(x => x.ID == ID);
            _db.ToDos.Remove(t);

            ///    LOG     ///
            var user = await _userManager.GetUserAsync(User);
            await _db.Logs.AddAsync(user.CreateLog("ToDo", $"Task: {t.Task}, was DELETED"));
            /// END OF LOG ///

            await _db.SaveChangesAsync();
            return RedirectToAction("index");
        }

        [HttpGet]
        [Authorize(Policy="Is Owner")]
        public async Task<IActionResult> DeleteAll ()
        {
            ViewBag.Todos = await _db.ToDos.ToListAsync();
            return View();
        }

        [HttpGet]
        [Authorize(Policy="Is Owner")]
        public async Task<IActionResult> DeleteAllOk (string code = null)
        {
            if (code == "dltok")
            {
                var t = await _db.ToDos.ToArrayAsync();
                _db.ToDos.RemoveRange(t);
                ///    LOG     ///
                var user = await _userManager.GetUserAsync(User);
                await _db.Logs.AddAsync(user.CreateLog("ToDo", $"All Tasks were DELETED"));
                /// END OF LOG ///
                await _db.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Authorize(Policy="Todo EditorKiller")]
        public async Task<IActionResult> Done (int? ID)
        {
            if (ID == null) 
            {
                return RedirectToAction("Index", "Todo");
            }

            ToDo t = await _db.ToDos.SingleOrDefaultAsync(x => x.ID == ID);
            t.Done = true;
            _db.ToDos.Update(t);
            ///    LOG     ///
            var user = await _userManager.GetUserAsync(User);
            await _db.Logs.AddAsync(user.CreateLog("ToDo", $"Task: {t.Task}, was marked as DONE"));
            /// END OF LOG ///
            await _db.SaveChangesAsync();

            return RedirectToAction("Index", "Todo");
        }

	}
}
