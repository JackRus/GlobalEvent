using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GlobalEvent.Data;
using GlobalEvent.Models.OwnerViewModels;
using GlobalEvent.Models.VisitorViewModels;
using Microsoft.EntityFrameworkCore;
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
	}
}
