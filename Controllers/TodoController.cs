using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GlobalEvent.Data;
using GlobalEvent.Models.AdminViewModels;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using GlobalEvent.Models;
using Microsoft.AspNetCore.Http;
using System;

namespace GlobalEvent.Controllers
{
	[Authorize]
    public class TodoController : Controller
    {
		private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private string _id;

		public TodoController (UserManager<ApplicationUser> userManager, ApplicationDbContext context, IHttpContextAccessor http)
        {
            _db = context;
            _userManager = userManager;
            _id = _userManager.GetUserId(http.HttpContext.User);
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
                t.Created = DateTime.Now.ToString("yyyy-MM-dd");
                await _db.ToDos.AddAsync(t);
                await _db.Logs.AddAsync(await Log.New("ToDo", $"Task: {t.Task}, was CREATED", _id, _db));

                return RedirectToAction("Index", url);
            }
            return RedirectToAction("Index", "Owner", new { message = "Couldn't create ToDo item. Please try again."});
        }
        

        [HttpGet]
        [Authorize(Policy="Todo Creator")]
        public async Task<IActionResult> AddFull() // from Todo/Index
        {
            ViewBag.EventList = await ToDo.GenerateEventList(_db);
            return View();
        }

        [HttpGet]
        [Authorize(Policy="Todo Creator")]
        public async Task<IActionResult> Copy (int? ID)
        {
            if (ID == null) 
            {
                return RedirectToAction("Index");
            }
            ViewBag.EventList = await ToDo.GenerateEventList(_db);
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
            ViewBag.EventList = await ToDo.GenerateEventList(_db);
            return View(await _db.ToDos.FirstOrDefaultAsync(x => x.ID == ID));
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        [Authorize(Policy="Todo EditorKiller")]
        public async Task<IActionResult> Edit (ToDo t)
        {
            if (ModelState.IsValid)
            {
                ToDo tOld = await _db.ToDos.FirstOrDefaultAsync(x => x.ID == t.ID);
                tOld.CopyValues(t, _db);
                await _db.Logs.AddAsync(await Log.New("ToDo", $"Task: {tOld.Task}, was EDITED", _id, _db));
            }

            return RedirectToAction("Index", "Todo");
        }

        [HttpGet]
        [Authorize(Policy="Todo EditorKiller")]
        public async Task<IActionResult> Delete (int? ID)
        {
            if (ID != null) 
            {
                ToDo t = await _db.ToDos.FirstOrDefaultAsync(x => x.ID == ID);
                _db.ToDos.Remove(t);
                await _db.Logs.AddAsync(await Log.New("ToDo", $"Task: {t.Task}, was DELETED", _id, _db));
            }

            return RedirectToAction("index", "Todo");
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
        public async Task<IActionResult> DeleteAllOk ()
        {
            var t = await _db.ToDos.ToArrayAsync();
            _db.ToDos.RemoveRange(t);
            await _db.Logs.AddAsync(await Log.New("ToDo", $"All Tasks were DELETED", _id, _db));

            return RedirectToAction("Index", "Todo");
        }

        [HttpGet]
        [Authorize(Policy="Todo EditorKiller")]
        public async Task<IActionResult> Done (int? ID)
        {
            if (ID != null) 
            {
                ToDo t = await _db.ToDos.SingleOrDefaultAsync(x => x.ID == ID);
                t.Done = true;
                _db.ToDos.Update(t);
                await _db.Logs.AddAsync(await Log.New("ToDo", $"Task: {t.Task}, was marked as DONE", _id, _db));
            }

            return RedirectToAction("Index", "Todo");
        }

	}
}
