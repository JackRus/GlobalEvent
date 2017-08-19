using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GlobalEvent.Data;
using GlobalEvent.Models;
using GlobalEvent.Models.OwnerViewModels;
using GlobalEvent.Models.VisitorViewModels;
using Microsoft.AspNetCore.Authorization;
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

		public VTypeController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
		{
			_db = context;
			_userManager = userManager;
		}

		[HttpGet]
		[Authorize(Policy = "VTypes Viewer")]
		public async Task<IActionResult> Index(int? ID) //displays all Products
		{
			if (ID == null)
			{
				return RedirectToAction("Events", "Owner");
			}

			ViewBag.VTypes = await _db.Types
				.Where(x => x.EID == ID)
				.ToListAsync();
			ViewBag.ID = ID;
			ViewBag.Event = (await _db.Events.SingleOrDefaultAsync(x => x.ID == ID)).Name;
			return View();
		}

		[HttpGet]
		[Authorize(Policy = "VType Creator")]
		public async Task<IActionResult> Add(int? ID)
		{
			if (ID == null)
			{
				return RedirectToAction("Events", "Owner");
			}
			ViewBag.Event = (await _db.Events.SingleOrDefaultAsync(x => x.ID == ID)).Name;
			return View(new VType() { EID = (int)ID });
		}

		[HttpPost]
		[AutoValidateAntiforgeryToken]
		[Authorize(Policy = "VType Creator")]
		public async Task<IActionResult> Add(VType v)
		{
			// reset Product ID to 0
			v.ID = 0;
			if (ModelState.IsValid)
			{
				Event e = await _db.Events.FirstOrDefaultAsync(x => x.ID == v.EID);
				e.Types.Add(v);
				_db.Events.Update(e);
				///    LOG     ///
				var user = await _userManager.GetUserAsync(User);
				await _db.Logs.AddAsync(user.CreateLog("VType", $"Visitor Type: {v.Name}, was CREATED"));
				/// END OF LOG ///
				await _db.SaveChangesAsync();

				return RedirectToAction("Index", new { ID = v.EID });
			}
			return RedirectToAction("Events", "Owner");
		}

		[HttpGet]
		[Authorize(Policy = "VType Creator")]
		public async Task<IActionResult> Copy(int? ID)
		{
			if (ID == null)
			{
				return RedirectToAction("Events", "Owner");
			}
			ViewBag.Event = (await _db.Events.SingleOrDefaultAsync(x => x.ID == ID)).Name;
			return View(await _db.Types.FirstOrDefaultAsync(x => x.ID == ID));
		}

		[HttpPost]
		[AutoValidateAntiforgeryToken]
		[Authorize(Policy = "VType Creator")]
		public async Task<IActionResult> Copy(VType v)
		{
			// reset VType ID to 0 si
			v.ID = 0;
			if (ModelState.IsValid)
			{
				Event e = await _db.Events.FirstOrDefaultAsync(x => x.ID == v.EID);
				e.Types.Add(v);
				_db.Events.Update(e);
				///    LOG     ///
				var user = await _userManager.GetUserAsync(User);
				await _db.Logs.AddAsync(user.CreateLog("VType", $"Visitor Type: {v.Name}, was COPIED"));
				/// END OF LOG ///
				await _db.SaveChangesAsync();
				return RedirectToAction("Index", new { ID = v.EID });
			}

			return RedirectToAction("Events", "Owner");
		}

		[HttpGet]
		[Authorize(Policy = "VType Creator")]
		public async Task<IActionResult> Edit(int? ID)
		{
			if (ID == null)
			{
				return RedirectToAction("Events");
			}
			ViewBag.Event = (await _db.Events.SingleOrDefaultAsync(x => x.ID == ID)).Name;
			return View(await _db.Types.FirstOrDefaultAsync(x => x.ID == ID));
		}

		[HttpPost]
		[AutoValidateAntiforgeryToken]
		[Authorize(Policy = "VType Creator")]
		public async Task<IActionResult> Edit(VType v)
		{
			if (ModelState.IsValid)
			{
				_db.Types.Update(v);
				///    LOG     ///
				var user = await _userManager.GetUserAsync(User);
				await _db.Logs.AddAsync(user.CreateLog("VType", $"Visitor Type: {v.Name}, was EDITED"));
				/// END OF LOG ///
				await _db.SaveChangesAsync();

				return RedirectToAction("Index", new { ID = v.EID });
			}

			return RedirectToAction("Events", "Owner");
		}

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

		[HttpGet]
		[Authorize(Policy = "VType Killer")]
		public async Task<IActionResult> DeleteOk(int? ID)
		{
			if (ID == null)
			{
				return RedirectToAction("Events", "Owner");
			}

			// extrats event with the matching ID
			VType v = await _db.Types.FirstOrDefaultAsync(x => x.ID == ID);
			_db.Types.Remove(v);
			///    LOG     ///
			var user = await _userManager.GetUserAsync(User);
			await _db.Logs.AddAsync(user.CreateLog("VType", $"Visitor Type: {v.Name}, was DELETED"));
			/// END OF LOG ///
			await _db.SaveChangesAsync();

			return RedirectToAction("Index", new { ID = v.EID });
		}

		[HttpGet]
		[Authorize(Policy = "Is Owner")]
		public async Task<IActionResult> DeleteAll(int? ID)
		{
			if (ID == null)
			{
				return RedirectToAction("Events", "Owner");
			}

			// confirmation page
			Event e = await _db.Events
				.Include(x => x.Types)
				.FirstOrDefaultAsync(x => x.ID == ID);

			return View(e);
		}

		[HttpGet]
		[Authorize(Policy = "Is Owner")]
		public async Task<IActionResult> DeleteAllOk(int? ID)
		{
			// redirects if no event ID provided || direct access
			if (ID == null)
			{
				return RedirectToAction("Events", "Owner");
			}

			// delete all Products for a specific event
			_db.Types.RemoveRange(_db.Types.Where(x => x.EID == ID));
            ///    LOG     ///
            var user = await _userManager.GetUserAsync(User);
            await _db.Logs.AddAsync(user.CreateLog("VType", $"All Visitor Types for Event ID: {ID}, were DELETED"));
            /// END OF LOG ///
			await _db.SaveChangesAsync();

			return RedirectToAction("Index", new { ID = ID });
		}
	}
}