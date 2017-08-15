using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GlobalEvent.Data;
using GlobalEvent.Models.AdminViewModels;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace GlobalEvent.Controllers
{
	[Authorize]
    public class TodoController : Controller
    {
		private readonly ApplicationDbContext _db;

		public TodoController (ApplicationDbContext context)
        {
            _db = context;
        }

        public IActionResult Index()
        {
            ViewBag.Todos = _db.ToDos
                .Where(x => !x.Done)
                .OrderBy(x => x.Deadline)
                .ToList();
            ViewBag.Done = _db.ToDos
                .Where(x => x.Done)
                .OrderBy(x => x.Deadline)
                .ToList();
            return View();
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Add (ToDo t)
        {
            if (ModelState.IsValid)
            {
                _db.ToDos.Add(t);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index", "Owner");
            }
            return RedirectToAction("Index", "Owner", new { message = "Couldn't create ToDo item. Please try again."});
        }

        public IActionResult AddFull()
        {
            return View(new ToDo());
        }

        [HttpGet]
        public async Task<IActionResult> Copy (int? ID)
        {
            if (ID == null) return RedirectToAction("Index");
            return View(await _db.ToDos.FirstOrDefaultAsync(x => x.ID == ID));
        }

        [HttpGet]
        public async Task<IActionResult> Edit (int? ID)
        {
            if (ID == null) return RedirectToAction("Index");
            return View(await _db.ToDos.FirstOrDefaultAsync(x => x.ID == ID));
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Edit (ToDo t)
        {
            if (ModelState.IsValid)
            {
                ToDo tOld = await _db.ToDos.FirstOrDefaultAsync(x => x.ID == t.ID);
                tOld.Task = t.Task;
                tOld.Comments = t.Comments;
                tOld.Done = t.Done;
                tOld.EID = t.EID;
                tOld.Deadline = t.Deadline;
                _db.ToDos.Update(tOld);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Delete (int? ID)
        {
            if (ID == null) return RedirectToAction("Index");

            ToDo t = await _db.ToDos.FirstOrDefaultAsync(x => x.ID == ID);
            _db.ToDos.Remove(t);
            await _db.SaveChangesAsync();
            return RedirectToAction("index");
        }

        public async Task<IActionResult> DeleteAll ()
        {
            ViewBag.Todos = await _db.ToDos.ToListAsync();
            return View();
        }

        public async Task<IActionResult> DeleteAllOk (string code = null)
        {
            if (code == "dltok")
            {
                var t = await _db.ToDos.ToArrayAsync();
                _db.ToDos.RemoveRange(t);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
	}
}
