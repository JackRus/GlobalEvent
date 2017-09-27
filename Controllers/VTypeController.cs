using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GlobalEvent.Data;
using GlobalEvent.Models;
using GlobalEvent.Models.AdminViewModels;
using GlobalEvent.Models.OwnerViewModels;
using GlobalEvent.Models.VisitorViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GlobalEvent.Controllers
{
	[Authorize]
	public class VTypeController : Controller
	{
		private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private string _id;

		public VTypeController (UserManager<ApplicationUser> userManager, ApplicationDbContext context, IHttpContextAccessor http)
        {
            _db = context;
            _userManager = userManager;
            _id = _userManager.GetUserId(http.HttpContext.User);
        }

		//
		// GET: VType/Index
		[HttpGet]
		[Authorize(Policy = "VTypes Viewer")]
		public async Task<IActionResult> Index(int? ID)
		{
			if (ID == null)
			{
				return RedirectToAction("Events", "Owner");
			}

			ViewBag.Event = await _db.Events.Include(x => x.Types).SingleOrDefaultAsync(x => x.ID == ID);
			
			return View();
		}

		//
		// GET: VType/Add
		[HttpGet]
		[Authorize(Policy = "VType Creator")]
		public async Task<IActionResult> Add(int? ID)
		{
			if (ID == null)
			{
				return RedirectToAction("Events", "Owner");
			}
			ViewBag.Event = await _db.Events.SingleOrDefaultAsync(x => x.ID == ID);
			
			return View();
		}

		//
		// POST: VType/Add
		[HttpPost]
		[AutoValidateAntiforgeryToken]
		[Authorize(Policy = "VType Creator")]
		public async Task<IActionResult> Add(VType model)
		{
			if (ModelState.IsValid)
			{
				Event eventToUpdate = await _db.Events.FirstOrDefaultAsync(x => x.ID == model.EID);
				model.ID = 0; // reset Product ID
				eventToUpdate.Types.Add(model);
				_db.Events.Update(eventToUpdate);
				
				// add log for visitor
				await _db.Logs.AddAsync(await Log.New("VType", $"Visitor Type: {model.Name}, was CREATED", _id, _db));

				return RedirectToAction("Index", new { ID = model.EID });
			}
			return RedirectToAction("Events", "Owner");
		}

		//
		// GET: VType/Copy
		[HttpGet]
		[Authorize(Policy = "VType Creator")]
		public async Task<IActionResult> Copy(int? ID)
		{
			if (ID == null)
			{
				return RedirectToAction("Events", "Owner");
			}
			
			VType type = await _db.Types.SingleOrDefaultAsync(x => x.ID == ID);
			ViewBag.Event = (await _db.Events.SingleOrDefaultAsync(x => x.ID == type.EID)).Name;
			
			return View(type);
		}

		//
		// POST: VType/Copy
		[HttpPost]
		[AutoValidateAntiforgeryToken]
		[Authorize(Policy = "VType Creator")]
		public async Task<IActionResult> Copy(VType model)
		{
			if (ModelState.IsValid)
			{
				Event eventToUpdate = await _db.Events.FirstOrDefaultAsync(x => x.ID == model.EID);
				model.ID = 0; // reset VType ID
				eventToUpdate.Types.Add(model);
				_db.Events.Update(eventToUpdate);

				// add log for visitor
				await _db.Logs.AddAsync(await Log.New("VType", $"Visitor Type: {model.Name}, was COPIED", _id, _db));

				return RedirectToAction("Index", new { ID = model.EID });
			}

			return RedirectToAction("Events", "Owner");
		}

		//
		// GET: VType/Edit
		[HttpGet]
		[Authorize(Policy = "VType Creator")]
		public async Task<IActionResult> Edit(int? ID)
		{
			if (ID == null)
			{
				return RedirectToAction("Events");
			}
			
			VType type = await _db.Types.SingleOrDefaultAsync(x => x.ID == ID);
			ViewBag.Event = (await _db.Events.SingleOrDefaultAsync(x => x.ID == type.EID)).Name;

			return View(type);
		}

		//
		// POST: VType/Edit
		[HttpPost]
		[AutoValidateAntiforgeryToken]
		[Authorize(Policy = "VType Creator")]
		public async Task<IActionResult> Edit(VType model)
		{
			if (ModelState.IsValid)
			{
				_db.Types.Update(model);
				
				// log for visitor
				await _db.Logs.AddAsync(await Log.New("VType", $"Visitor Type: {model.Name}, was EDITED", _id, _db));

				return RedirectToAction("Index", new { ID = model.EID });
			}

			return RedirectToAction("Events", "Owner");
		}

		//
		// GET: VType/Delete
		[HttpGet]
		[Authorize(Policy = "VType Killer")]
		public async Task<IActionResult> Delete(int? ID)
		{
			if (ID == null)
			{
				return RedirectToAction("Events", "Owner");
			}
			
			return View(await _db.Types.FirstOrDefaultAsync(x => x.ID == ID));
		}

		//
		// GET: VType/DeleteOk
		[HttpGet]
		[Authorize(Policy = "VType Killer")]
		public async Task<IActionResult> DeleteOk(int? ID)
		{
			if (ID == null)
			{
				return RedirectToAction("Events", "Owner");
			}

			VType type = await _db.Types.SingleOrDefaultAsync(x => x.ID == ID);
			_db.Types.Remove(type);
			
			// log for visitor
			await _db.Logs.AddAsync(await Log.New("VType", $"Visitor Type: {type.Name}, was DELETED", _id, _db));

			return RedirectToAction("Index", new { ID = type.EID });
		}

		// 
		// GET: VType/DeleteAll
		[HttpGet]
		[Authorize(Policy = "Is Owner")]
		public async Task<IActionResult> DeleteAll(int? ID)
		{
			if (ID == null)
			{
				return RedirectToAction("Events", "Owner");
			}
			Event eventToUpdate = await _db.Events.Include(x => x.Types).FirstOrDefaultAsync(x => x.ID == ID);

			return View(eventToUpdate);
		}

		//
		// GET: VType/DeleteAllOk
		[HttpGet]
		[Authorize(Policy = "Is Owner")]
		public async Task<IActionResult> DeleteAllOk(int? ID)
		{
			if (ID == null)
			{
				return RedirectToAction("Events", "Owner");
			}

			_db.Types.RemoveRange(_db.Types.Where(x => x.EID == ID));
            
			// log for visitor
			await _db.Logs.AddAsync(await Log.New("VType", $"All Visitor Types for Event ID: {ID}, were DELETED", _id, _db));

			return RedirectToAction("Index", new { ID = ID });
		}
	}
}