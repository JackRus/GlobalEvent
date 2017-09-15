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

        //
        // GET: Request/Add
        [HttpGet]
        public async Task<IActionResult> Add(int? ID)
        {
            if (ID == null) 
            {
                return RedirectToAction("Dashboard", "Admin");
            }

            // get current visitor
            ViewBag.Visitor = await _db.Visitors.SingleOrDefaultAsync(x => x.ID == ID);

            return View();
        }
        
        //
        // POST: Request/Add
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Add (Request model)
        {
            if (string.IsNullOrEmpty(model.Description) || model.VID == 0)
            {
                return RedirectToAction("Dashboard", "Admin");
            }

            // get current admin
            ApplicationUser user = await _userManager.GetUserAsync(User);
            
            // update values
            model.AdminID = user.Id;
            model.AdminName = $"{user.FirstName} {user.LastName}";

            // get current visitor, and add request
            Visitor v = await _db.Visitors.SingleOrDefaultAsync(x => x.ID == model.VID);
            v.Requests.Add(model);
            _db.Visitors.Update(v);

            // log for admin
            await _db.Logs.AddAsync(await Log.New("Request", $"Request: \"{model.Description}\" for VID: {model.VID}, was CREATED", _id, _db));

            return RedirectToAction("ViewVisitor", "Admin", new {ID = model.VID});
        }

        //
        // GET: Request/Edit
        [HttpGet]
        public async Task<IActionResult> Edit (int? ID)
        {
            if (ID == null) 
            {
                return RedirectToAction("Index");
            }
            
            // get request by id
            Request request = await _db.Requests.SingleOrDefaultAsync(x => x.ID == ID);
            // get visitor for the request
            ViewBag.Visitor = await _db.Visitors.SingleOrDefaultAsync(x => x.ID == request.VID);

            return View(request);
        }

        //
        // POST: Request/Edit
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Edit (Request model)
        {
			if (string.IsNullOrEmpty(model.Description) || model.ID == 0)
            {
                return RedirectToAction("Dashboard", "Admin");
            }

            // get visitor ID and update request's values in db
            int VID = await model.CopyValues(_db);

            // log for admin
            await _db.Logs.AddAsync(await Log.New("Request", $"Request: \"{model.Description}\" for VID: {model.VID}, was EDITED", _id, _db));

            return RedirectToAction("ViewVisitor", "Admin", new {ID = VID});
        }

        //
        // GET: Request/Delete
        [HttpGet]
        public async Task<IActionResult> Delete (int? ID)
        {
            if (ID == null) 
            {
                return RedirectToAction("Dashboard", "Admin");
            }

            // find and delete request
            Request request = await _db.Requests.SingleOrDefaultAsync(x => x.ID == ID);
            _db.Requests.Remove(request);

            // log for admin
            await _db.Logs.AddAsync(await Log.New("Request", $"Request: \"{request.Description}\" for VID: {request.VID}, was DELETED", _id, _db));

            return RedirectToAction("ViewVisitor", "Admin", new {ID = request.VID});
        }

        //
        // GET: Request/Status
        [HttpGet]
        public async Task<IActionResult> Status (int? ID, string Name = null)
        {
            if (ID == null || Name == null)
            {
                return RedirectToAction("Dashboard", "Admin");
            }

            // find and update values
            Request r = await _db.Requests.FirstOrDefaultAsync(x => x.ID == ID);
            r.UpdateValue(Name, _db);

            // log for admin
            await _db.Logs.AddAsync(await Log.New("Request", $"Request: \"{r.Description}\" for VID: {r.VID}, feild {Name} was changed", _id, _db));

            return RedirectToAction("ViewVisitor", "Admin", new {ID = r.VID});
        }
	}
}
