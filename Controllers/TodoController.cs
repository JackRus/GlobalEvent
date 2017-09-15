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

        //
        // GET: Todo/Index
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

        //
        // POST: Todo/Add
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        [Authorize(Policy="Todo Creator")]
        public async Task<IActionResult> Add (ToDo todo, string url = "Owner")
        {
            if (ModelState.IsValid)
            {
                todo.Created = DateTime.Now.ToString("yyyy-MM-dd");
                await _db.ToDos.AddAsync(todo);
                
                // log for admin
                await _db.Logs.AddAsync(await Log.New("ToDo", $"Task: {todo.Task}, was CREATED", _id, _db));

                return RedirectToAction("Index", url);
            }

            return RedirectToAction("Index", "Owner", new { message = "Couldn't create ToDo item. Please try again."});
        }
        

        //
        // GET: Todo/AddFull
        [HttpGet]
        [Authorize(Policy="Todo Creator")]
        public async Task<IActionResult> AddFull() // from Todo/Index
        {
            ViewBag.EventList = await ToDo.GenerateEventList(_db);
            return View();
        }

        //
        // GET: Todo/Copy
        [HttpGet]
        [Authorize(Policy="Todo Creator")]
        public async Task<IActionResult> Copy (int? ID)
        {
            if (ID == null) 
            {
                return RedirectToAction("Index");
            }

            ViewBag.EventList = await ToDo.GenerateEventList(_db);
            ToDo todo = await _db.ToDos.FirstOrDefaultAsync(x => x.ID == ID);

            return View(todo);
        }

        //
        // GET: Todo/Edit
        [HttpGet]
        [Authorize(Policy="Todo EditorKiller")]
        public async Task<IActionResult> Edit (int? ID)
        {
            if (ID == null) 
            {
                return RedirectToAction("Index");
            }

            ViewBag.EventList = await ToDo.GenerateEventList(_db);
            ToDo todo = await _db.ToDos.FirstOrDefaultAsync(x => x.ID == ID);

            return View(todo);
        }

        //
        // POST: Todo/Edit
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        [Authorize(Policy="Todo EditorKiller")]
        public async Task<IActionResult> Edit (ToDo model)
        {
            if (ModelState.IsValid)
            {
                ToDo todo = await _db.ToDos.SingleOrDefaultAsync(x => x.ID == model.ID);
                todo.CopyValues(model, _db);
                
                // Log for admin
                await _db.Logs.AddAsync(await Log.New("ToDo", $"Task: {todo.Task}, was EDITED", _id, _db));
            }

            return RedirectToAction("Index", "Todo");
        }

        //
        //
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
