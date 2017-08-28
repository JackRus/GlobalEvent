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
    public class NoteController : Controller
    {
		private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private string _id;

		public NoteController (UserManager<ApplicationUser> userManager, ApplicationDbContext context, IHttpContextAccessor http)
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
            return View(new Note());
        }
        
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Add (Note n)
        {
            if (string.IsNullOrEmpty(n.Description) || n.VID == 0)
            {
                return RedirectToAction("Dashboard", "Admin");
            }
            
            var user = await _userManager.GetUserAsync(User);
            n.AdminID = user.Id;
            n.AdminName = $"{user.FirstName} {user.LastName}";

            Visitor v = await _db.Visitors.SingleOrDefaultAsync(x => x.ID == n.VID);
            v.Notes.Add(n);
            _db.Visitors.Update(v);
            await _db.Logs.AddAsync(await Log.New("Note", $"Note: \"{n.Description}\" for VID: {n.VID}, was CREATED", _id, _db));
            
            return RedirectToAction("ViewVisitor", "Admin", new {ID = n.VID});
        }

        [HttpGet]
        public async Task<IActionResult> Edit (int? ID)
        {
            if (ID == null) 
            {
                return RedirectToAction("Index");
            }
            ViewBag.Visitor = await _db.Visitors.SingleOrDefaultAsync(x => x.ID == ID);
            return View(await _db.Notes.FirstOrDefaultAsync(x => x.ID == ID));
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Edit (Note n)
        {
            if (string.IsNullOrEmpty(n.Description) || n.ID == 0)
            {
                return RedirectToAction("Dashboard", "Admin");
            }
            var VID = await n.CopyValues(_db);

            await _db.Logs.AddAsync(await Log.New("Note", $"Note: \"{n.Description}\" for VID: {n.VID}, was EDITED", _id, _db));

            return RedirectToAction("ViewVisitor", "Admin", new {ID = VID});
        }

        [HttpGet]
        public async Task<IActionResult> Delete (int? ID)
        {
            if (ID == null) 
            {
                return RedirectToAction("Dashboard", "Admin");
            }

            Note n = await _db.Notes.FirstOrDefaultAsync(x => x.ID == ID);
            _db.Notes.Remove(n);
            await _db.Logs.AddAsync(await Log.New("Note", $"Note: \"{n.Description}\" for VID: {n.VID}, was DELETED", _id, _db));

            return RedirectToAction("ViewVisitor", "Admin", new {ID = n.VID});
        }

        [HttpGet]
        public async Task<IActionResult> Status (int? ID, string Name = null)
        {
            if (ID == null || Name == null)
            {
                return RedirectToAction("Dashboard", "Admin");
            }

            Note n = await _db.Notes.FirstOrDefaultAsync(x => x.ID == ID);
            n.UpdateValues(Name, _db);
            await _db.Logs.AddAsync(await Log.New("Note", $"Note: \"{n.Description}\" for VID: {n.VID}, feild {Name} was changed", _id, _db));

            return RedirectToAction("ViewVisitor", "Admin", new {ID = n.VID});
        }
	}
}
