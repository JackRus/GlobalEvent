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
    public class NoteController : Controller
    {
		private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

		public NoteController (UserManager<ApplicationUser> userManager, ApplicationDbContext context)
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

            ///    LOG     ///
            var user = await _userManager.GetUserAsync(User);
            await _db.Logs.AddAsync(user.CreateLog("Note", $"Note: \"{n.Description}\" for VID: {n.VID}, was CREATED"));
            /// END OF LOG ///

            n.AdminID = user.Id;
            n.AdminName = $"{user.FirstName} {user.LastName}";

            // update visitor + add Notes to db
            Visitor v = await _db.Visitors.SingleOrDefaultAsync(x => x.ID == n.VID);
            v.Notes.Add(n);
            _db.Visitors.Update(v);
            await _db.SaveChangesAsync();
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

            Note nOld = await _db.Notes.FirstOrDefaultAsync(x => x.ID == n.ID);
            nOld.Description = n.Description;
            nOld.SeenByAdmin = n.SeenByAdmin;
            nOld.Important = n.Important;
            _db.Notes.Update(nOld);

            ///    LOG     ///
            var user = await _userManager.GetUserAsync(User);
            await _db.Logs.AddAsync(user.CreateLog("Note", $"Note: \"{n.Description}\" for VID: {n.VID}, was EDITED"));
            /// END OF LOG ///

            await _db.SaveChangesAsync();
            return RedirectToAction("ViewVisitor", "Admin", new {ID = nOld.VID});
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

            ///    LOG     ///
            var user = await _userManager.GetUserAsync(User);
            await _db.Logs.AddAsync(user.CreateLog("Note", $"Note: \"{n.Description}\" for VID: {n.VID}, was DELETED"));
            /// END OF LOG ///

            await _db.SaveChangesAsync();
            return RedirectToAction("ViewVisitor", "Admin", new {ID = n.VID});
        }

        [HttpGet]
        public async Task<IActionResult> Status (int? ID, string Name = null)
        {
            if (ID == null || Name == null)
            {
                return RedirectToAction("Dashboard", "Admin");
            }

            // get Note by ID
            Note n = await _db.Notes.FirstOrDefaultAsync(x => x.ID == ID);
            
            switch (Name)
            {
                case "SEEN": 
                    n.SeenByAdmin = n.SeenByAdmin ? false : true;
                    break;
                case "IMPORTANT": 
                    n.Important = n.Important ? false : true;
                    break;
                default: 
                    break;
            }

            _db.Notes.Update(n);

            ///    LOG     ///
            var user = await _userManager.GetUserAsync(User);
            await _db.Logs.AddAsync(user.CreateLog("Note", $"Note: \"{n.Description}\" for VID: {n.VID}, feild {Name} was changed"));
            /// END OF LOG ///

            await _db.SaveChangesAsync();
            return RedirectToAction("ViewVisitor", "Admin", new {ID = n.VID});
        }
	}
}
