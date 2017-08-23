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

namespace GlobalEvent.Controllers
{
	[Authorize]
    public class IssueController : Controller
    {
		private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

		public IssueController (UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _db = context;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Add(int? ID, int? EID)
        {
            Issue i = new Issue();
            i.InitiateDescription(ID, EID);
            ViewBag.List = Issue.GenerateTypes();
            return View(i);
        }
        
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Add (Issue i)
        {
            if (string.IsNullOrEmpty(i.Description))
            {
                return RedirectToAction("Dashboard", "Admin");
            }
     
            ///    LOG     ///
            var user = await _userManager.GetUserAsync(User);
            await _db.Logs.AddAsync(user.CreateLog("Issue", $"Issue: \"{i.Description}\", was CREATED"));
            /// END OF LOG ///

            Issue newI = new Issue();
            newI.CopyInfo(i);
            newI.AdminID = user.Id;
            newI.AdminName = $"{user.FirstName} {user.LastName}";

            await _db.Issues.AddAsync(newI);   
            await _db.SaveChangesAsync();

            return RedirectToAction("");
        }

        // [HttpPost]
        // [AutoValidateAntiforgeryToken]
        // public async Task<IActionResult> Edit (Issue n)
        // {
        //     if (string.IsNullOrEmpty(n.Description) || n.ID == 0)
        //     {
        //         return RedirectToAction("Dashboard", "Admin");
        //     }

        //     Issue nOld = await _db.Issues.FirstOrDefaultAsync(x => x.ID == n.ID);
        //     nOld.Description = n.Description;
        //     nOld.SeenByAdmin = n.SeenByAdmin;
        //     nOld.Important = n.Important;
        //     _db.Issues.Update(nOld);

        //     ///    LOG     ///
        //     var user = await _userManager.GetUserAsync(User);
        //     await _db.Logs.AddAsync(user.CreateLog("Issue", $"Issue: \"{n.Description}\" for VID: {n.VID}, was EDITED"));
        //     /// END OF LOG ///

        //     await _db.SaveChangesAsync();
        //     return RedirectToAction("ViewVisitor", "Admin", new {ID = nOld.VID});
        // }

        // [HttpGet]
        // public async Task<IActionResult> Delete (int? ID)
        // {
        //     if (ID == null) 
        //     {
        //         return RedirectToAction("Dashboard", "Admin");
        //     }

        //     Issue n = await _db.Issues.FirstOrDefaultAsync(x => x.ID == ID);
        //     _db.Issues.Remove(n);

        //     ///    LOG     ///
        //     var user = await _userManager.GetUserAsync(User);
        //     await _db.Logs.AddAsync(user.CreateLog("Issue", $"Issue: \"{n.Description}\" for VID: {n.VID}, was DELETED"));
        //     /// END OF LOG ///

        //     await _db.SaveChangesAsync();
        //     return RedirectToAction("ViewVisitor", "Admin", new {ID = n.VID});
        // }

        // [HttpGet]
        // public async Task<IActionResult> Status (int? ID, string Name = null)
        // {
        //     if (ID == null || Name == null)
        //     {
        //         return RedirectToAction("Dashboard", "Admin");
        //     }

        //     // get Issue by ID
        //     Issue n = await _db.Issues.FirstOrDefaultAsync(x => x.ID == ID);
            
        //     switch (Name)
        //     {
        //         case "SEEN": 
        //             n.SeenByAdmin = n.SeenByAdmin ? false : true;
        //             break;
        //         case "IMPORTANT": 
        //             n.Important = n.Important ? false : true;
        //             break;
        //         default: 
        //             break;
        //     }

        //     _db.Issues.Update(n);

        //     ///    LOG     ///
        //     var user = await _userManager.GetUserAsync(User);
        //     await _db.Logs.AddAsync(user.CreateLog("Issue", $"Issue: \"{n.Description}\" for VID: {n.VID}, feild {Name} was changed"));
        //     /// END OF LOG ///

        //     await _db.SaveChangesAsync();
        //     return RedirectToAction("ViewVisitor", "Admin", new {ID = n.VID});
        // }
	}
}
