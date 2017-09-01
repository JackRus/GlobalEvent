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
using Microsoft.AspNetCore.Http;

namespace GlobalEvent.Controllers
{
	[Authorize(Policy="Visitor Editor")]
    public class RequestController : Controller
    {
		private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private string _id;

		public RequestController (UserManager<ApplicationUser> userManager, ApplicationDbContext context, IHttpContextAccessor http)
        {
            _db = context;
            _userManager = userManager;
            _id = _userManager.GetUserId(http.HttpContext.User);
        }

        [HttpGet]
        public async Task<IActionResult> Add(int? ID)
        {
            if (ID == null) 
            {
                return RedirectToAction("Dashboard", "Admin");
            }

            ViewBag.Visitor = await _db.Visitors.SingleOrDefaultAsync(x => x.ID == ID);
            return View();
        }
        
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Add (Request r)
        {
            if (string.IsNullOrEmpty(r.Description) || r.VID == 0)
            {
                return RedirectToAction("Dashboard", "Admin");
            }

            ApplicationUser user = await _userManager.GetUserAsync(User);
            r.AdminID = user.Id;
            r.AdminName = $"{user.FirstName} {user.LastName}";
            Visitor v = await _db.Visitors.SingleOrDefaultAsync(x => x.ID == r.VID);
            v.Requests.Add(r);
            _db.Visitors.Update(v);

            await _db.Logs.AddAsync(await Log.New("Request", $"Request: \"{r.Description}\" for VID: {r.VID}, was CREATED", _id, _db));

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

            int VID = await r.CopyValues(_db);
            await _db.Logs.AddAsync(await Log.New("Request", $"Request: \"{r.Description}\" for VID: {r.VID}, was EDITED", _id, _db));

            return RedirectToAction("ViewVisitor", "Admin", new {ID = VID});
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
            await _db.Logs.AddAsync(await Log.New("Request", $"Request: \"{r.Description}\" for VID: {r.VID}, was DELETED", _id, _db));

            return RedirectToAction("ViewVisitor", "Admin", new {ID = r.VID});
        }

        [HttpGet]
        public async Task<IActionResult> Status (int? ID, string Name = null)
        {
            if (ID == null || Name == null)
            {
                return RedirectToAction("Dashboard", "Admin");
            }

            Request r = await _db.Requests.FirstOrDefaultAsync(x => x.ID == ID);
            r.UpdateValue(Name, _db);
            await _db.Logs.AddAsync(await Log.New("Request", $"Request: \"{r.Description}\" for VID: {r.VID}, feild {Name} was changed", _id, _db));

            return RedirectToAction("ViewVisitor", "Admin", new {ID = r.VID});
        }
	}
}
