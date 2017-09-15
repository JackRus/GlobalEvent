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

        //
        // GET: Note/Add
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
        
        //
        // POST: Note/Add
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Add (Note note)
        {
            if (string.IsNullOrEmpty(note.Description) || note.VID == 0)
            {
                return RedirectToAction("Dashboard", "Admin");
            }
            
            // get admin
            ApplicationUser user = await _userManager.GetUserAsync(User);
            note.AdminID = user.Id;
            note.AdminName = $"{user.FirstName} {user.LastName}";

            // find visitor and add note
            Visitor visitor = await _db.Visitors.SingleOrDefaultAsync(x => x.ID == note.VID);
            visitor.Notes.Add(note);
            _db.Visitors.Update(visitor);

            // log for admin
            await _db.Logs.AddAsync(await Log.New("Note", $"Note: \"{note.Description}\" for VID: {note.VID}, was CREATED", _id, _db));
            
            return RedirectToAction("ViewVisitor", "Admin", new {ID = note.VID});
        }

        //
        // GET: Note/Edit
        [HttpGet]
        public async Task<IActionResult> Edit (int? ID)
        {
            if (ID == null) 
            {
                return RedirectToAction("Index");
            }

            // get note by id
            Note note = await _db.Notes.FirstOrDefaultAsync(x => x.ID == ID);
            ViewBag.Visitor = await _db.Visitors.SingleOrDefaultAsync(x => x.ID == note.VID);
        
            return View(note);
        }

        //
        // POST: Note/Edit
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Edit (Note note)
        {
            if (string.IsNullOrEmpty(note.Description) || note.ID == 0)
            {
                return RedirectToAction("Dashboard", "Admin");
            }

            // get visitor ID and copy values
            int VID = await note.CopyValues(_db);

            // log for admin
            await _db.Logs.AddAsync(await Log.New("Note", $"Note: \"{note.Description}\" for VID: {note.VID}, was EDITED", _id, _db));

            return RedirectToAction("ViewVisitor", "Admin", new {ID = VID});
        }

        //
        // GET: Note/Delete
        [HttpGet]
        public async Task<IActionResult> Delete (int? ID)
        {
            if (ID == null) 
            {
                return RedirectToAction("Dashboard", "Admin");
            }

            // find and delete note
            Note note = await _db.Notes.FirstOrDefaultAsync(x => x.ID == ID);
            _db.Notes.Remove(note);

            // log for admin
            await _db.Logs.AddAsync(await Log.New("Note", $"Note: \"{note.Description}\" for VID: {note.VID}, was DELETED", _id, _db));

            return RedirectToAction("ViewVisitor", "Admin", new {ID = note.VID});
        }

        //
        // GET: Note/Status
        [HttpGet]
        public async Task<IActionResult> Status (int? ID, string Name = null)
        {
            if (ID == null || Name == null)
            {
                return RedirectToAction("Dashboard", "Admin");
            }

            // find note and update status
            Note note = await _db.Notes.FirstOrDefaultAsync(x => x.ID == ID);
            note.UpdateValues(Name, _db);

            // Log for admin
            await _db.Logs.AddAsync(await Log.New("Note", $"Note: \"{note.Description}\" for VID: {note.VID}, feild {Name} was changed", _id, _db));

            return RedirectToAction("ViewVisitor", "Admin", new {ID = note.VID});
        }
	}
}
