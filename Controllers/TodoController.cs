using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GlobalEvent.Data;
using GlobalEvent.Models.AdminViewModels;
using System.Threading.Tasks;

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
	}
}
