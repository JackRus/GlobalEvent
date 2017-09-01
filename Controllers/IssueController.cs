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
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace GlobalEvent.Controllers
{
    [Authorize(Policy="Visitor Editor")]
    public class IssueController : Controller
    {
		private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private string _id;

		public IssueController (UserManager<ApplicationUser> userManager, ApplicationDbContext context, IHttpContextAccessor http)
        {
            _db = context;
            _userManager = userManager;
            _id = _userManager.GetUserId(http.HttpContext.User);
        }

        // GET ADD
        [HttpGet]
        public IActionResult Add(int? ID, int? EID)
        {
            Issue i = new Issue();
            i.InitiateDescription(ID, EID); // based on the info provided
            ViewBag.List = Issue.GenerateTypes();
            return View(i);
        }
        
        /// POST ADD
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Add (Issue i)
        {
            var result = "Could't add Issue record.";
            if (!string.IsNullOrEmpty(i.Description))
            {
                i.Complete(await _userManager.GetUserAsync(User));
                await _db.Issues.AddAsync(i);   
                await _db.Logs.AddAsync(await Log.New("Issue", $"Issue: \"{i.Description}\", was CREATED", _id, _db));
                result = $"Issue record \"{i.Description}\" was created.";
            }
            return RedirectToAction("Dashboard", "Admin", new {message = result});
        }

        // GET EDIT
        [HttpGet]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Edit (int? ID)
        {
            if (ID == null)
            {
                return RedirectToAction("Dashboard", "Admin", new {message = $"Could't access Issue record."});
            }
            
            ViewBag.List = Issue.GenerateTypes();
            Issue i = await _db.Issues.SingleOrDefaultAsync(x => x.ID == ID);
            return View(i);
        }
        
        // POST EDIT
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Edit (Issue i)
        {
            var result = "Could't edit Issue record.";
            if (ModelState.IsValid)
            {
                Issue oldI = await _db.Issues.FirstOrDefaultAsync(x => x.ID == i.ID);
                oldI.CopyInfo(i);
                _db.Issues.Update(oldI);
                await _db.Logs.AddAsync(await Log.New("Issue", $"Issue: \"{i.Description}\", was EDITED", _id, _db));
                result = $"Issue record was edited.";
            }
            return RedirectToAction("Dashboard", "Admin", new {message = result});
        }

        // GET DELETE
        [HttpGet]
        public async Task<IActionResult> Delete (int? ID)
        {
            var result = "Could't delete Issue record.";
            if (ID != null) 
            {
                Issue i = await _db.Issues.SingleOrDefaultAsync(x => x.ID == ID);
                _db.Issues.Remove(i);
                await _db.Logs.AddAsync(await Log.New("Issue", $"Issue: \"{i.Description}\", was DELETED", _id, _db));
                result = $"Issue record \"{i.Description}\" was deleted.";
            }
            
            return RedirectToAction("Dashboard", "Admin", new {message = result});
        }

        [HttpGet]
        public async Task<IActionResult> Status (int? ID, string Name = null)
        {
            var result = $"Could't change \"{Name}\" value.";
            if (ID != null && Name != null)
            {
                Issue i = await _db.Issues.FirstOrDefaultAsync(x => x.ID == ID);
                i.UpdateValues(Name, _db);
                await _db.Logs.AddAsync(await Log.New("Issue", $"Issue: \"{i.Description}\", feild {Name} was changed.", _id, _db));
                result = $"Value of \"{Name}\" was changed.";
            }
            return RedirectToAction("Dashboard", "Admin", new {message = result});
        }
	}
}
