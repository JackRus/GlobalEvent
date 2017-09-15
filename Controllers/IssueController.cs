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

        //
        // GET: Issue/Add
        [HttpGet]
        public IActionResult Add(int? ID, int? EID)
        {
            Issue issue = new Issue();
            
            // create description
            issue.InitiateDescription(ID, EID); 

            // list of types for <select> tag
            ViewBag.List = Issue.GenerateTypes();
            return View(issue);
        }
        
        //
        // POST: Issue/Add
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Add (Issue issue)
        {
            // default message
            var result = "Could't add Issue record.";

            if (!string.IsNullOrEmpty(issue.Description))
            {
                // complete the model and add to db
                issue.Complete(await _userManager.GetUserAsync(User));
                await _db.Issues.AddAsync(issue);   
                
                // log for admin
                await _db.Logs.AddAsync(await Log.New("Issue", $"Issue: \"{issue.Description}\", was CREATED", _id, _db));

                result = $"Issue record \"{issue.Description}\" was created.";
            }
            return RedirectToAction("Dashboard", "Admin", new {message = result});
        }

        //
        // GET: Issue/Edit
        [HttpGet]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Edit (int? ID)
        {
            if (ID == null)
            {
                return RedirectToAction("Dashboard", "Admin", new {message = $"Could't access Issue record."});
            }
            
             // list of types for <select> tag
            ViewBag.List = Issue.GenerateTypes();
            Issue issue = await _db.Issues.SingleOrDefaultAsync(x => x.ID == ID);

            return View(issue);
        }
        
        //
        // POST: Issue/Edit
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Edit (Issue i)
        {
            // default message
            var result = "Could't edit Issue record.";

            if (ModelState.IsValid)
            {
                // update issue 
                Issue issueToUpdate = await _db.Issues.FirstOrDefaultAsync(x => x.ID == i.ID);
                issueToUpdate.CopyInfo(i);
                _db.Issues.Update(issueToUpdate);

                // log for admin
                await _db.Logs.AddAsync(await Log.New("Issue", $"Issue: \"{i.Description}\", was EDITED", _id, _db));

                result = $"Issue record was edited.";
            }

            return RedirectToAction("Dashboard", "Admin", new {message = result});
        }

        //
        // GET: Issue/Delete
        [HttpGet]
        public async Task<IActionResult> Delete (int? ID)
        {
            // default message
            var result = "Could't delete Issue record.";

            if (ID != null) 
            {
                // find and delete
                Issue issue = await _db.Issues.SingleOrDefaultAsync(x => x.ID == ID);
                _db.Issues.Remove(issue);

                // log for admin
                await _db.Logs.AddAsync(await Log.New("Issue", $"Issue: \"{issue.Description}\", was DELETED", _id, _db));

                result = $"Issue record \"{issue.Description}\" was deleted.";
            }
            
            return RedirectToAction("Dashboard", "Admin", new {message = result});
        }

        [HttpGet]
        public async Task<IActionResult> Status (int? ID, string Name = null)
        {
            // default message
            var result = $"Could't change \"{Name}\" value.";
            
            if (ID != null && Name != null)
            {
                // find and update
                Issue i = await _db.Issues.FirstOrDefaultAsync(x => x.ID == ID);
                i.UpdateValues(Name, _db);

                // log for admin
                await _db.Logs.AddAsync(await Log.New("Issue", $"Issue: \"{i.Description}\", feild {Name} was changed.", _id, _db));
                
                result = $"Value of \"{Name}\" was changed.";
            }
            return RedirectToAction("Dashboard", "Admin", new {message = result});
        }
	}
}
