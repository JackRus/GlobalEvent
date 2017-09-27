using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GlobalEvent.Data;
using GlobalEvent.Models.VisitorViewModels;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System;
using GlobalEvent.Models.OwnerViewModels;
using GlobalEvent.Models.AdminViewModels;

namespace GlobalEvent.Controllers
{
	public class VisitorController : Controller
	{
		private readonly ApplicationDbContext _db;

		public VisitorController(ApplicationDbContext context)
		{
			_db = context;
		}

		//
		// GET: Visitor/CheckInConfirmation 
		[HttpGet]
		public IActionResult CheckInConfirmation(int? EID)
		{
			if (EID == null) 
			{
				return RedirectToAction("Welcome", "Home");
			}
			ViewBag.EID = (int)EID;

			return View();
		}

		//
		// GET: Visitor/PreCheckIn
		[HttpGet]
		public IActionResult PreCheckIn(int? EID)
		{
			if (EID == null) 
			{
				return RedirectToAction("Welcome", "Home");
			}

			return View(new Visitor(){EID = (int)EID});
		}

		//
		// POST: Visitor/CheckIn
		[HttpPost]
		[AutoValidateAntiforgeryToken]
		public async Task<IActionResult> CheckIn(Visitor model)
		{
			if (model.RegistrationNumber == null || model.EID == 0) 
			{
				return RedirectToAction("Menu", "Home", new { EID = model.EID});
			}

			Visitor newVisitor = await _db.Visitors.SingleOrDefaultAsync(x => x.RegistrationNumber == model.RegistrationNumber);

			if (newVisitor != null)
			{
				if (newVisitor.CheckIned)
				{
					ViewBag.Message = "This Registration number was already used for Check In and name tag was printed. If you would like to change some information displayed on the name tag, please refer to the administrator.";
					ViewBag.EID = model.EID;
					return View("PreCheckIn", model);
				}

				ViewBag.Event = (await _db.Events.SingleOrDefaultAsync(x => x.ID == model.EID)).Name;
				
				// add lof for visitor
				newVisitor.AddLog("Check In", "Check-In Started", true);
				await _db.SaveChangesAsync();

				return View(newVisitor);
			}
			ViewBag.Message = "This Registration number isn't correct. Please try again.";

            return View("PreCheckIn", model);
		}

		//
		// GET: Visitor/CheckInOk
		[HttpGet]
		public async Task<IActionResult> CheckInOk (string number, int? EID)
		{
			if (number == null || EID == null) 
			{
				return RedirectToAction("Welcome", "Home");
			}
			
			Visitor visitor = await _db.Visitors.SingleOrDefaultAsync(x => x.RegistrationNumber == number);

			if (visitor == null || visitor.CheckIned)
			{
				ViewBag.Message = "Something went wrong. Please try again.";
				return RedirectToAction("Menu", "Home", new { EID = EID });
			}
			
			visitor.CompleteCheckIn();
			
			// add log for visitor
			visitor.AddLog("Check In", "Check-In Completed. Tag Printed", true);
			_db.Visitors.Update(visitor);
			await _db.SaveChangesAsync();

			return View();
		}

		//
		// GET: Visitor/Edit
		[HttpGet]
		public async Task<IActionResult> Edit (string number)
		{
			Visitor visitor = await _db.Visitors.FirstOrDefaultAsync(x => x.RegistrationNumber == number); 
			if (number == null || visitor == null || visitor.CheckIned)
			{
				return RedirectToAction("Welcome", "Home");
			}

			// add log for visitor
			visitor.AddLog("Check In", "Edition Started", true);
			_db.Visitors.Update(visitor);
			await _db.SaveChangesAsync();

			return View(visitor);
		}

		//
		// POST: Visitor/Post
		[HttpPost]
		[AutoValidateAntiforgeryToken]
		public async Task<IActionResult> EditOk(Visitor model)
		{
			if (!ModelState.IsValid) 
			{
				return RedirectToAction("Menu", "Home", new {EID = model.EID});
			}
			Visitor oldVisitor = await model.CopyValues(_db);
			
			// add log for visitor
			oldVisitor.AddLog("Check In", "Edition Completed", true);
			_db.Visitors.Update(oldVisitor);
			await _db.SaveChangesAsync();

			return View(oldVisitor);
		}

		//
		// GET: Visitor/PreRegister
		[HttpGet]
		public async Task<IActionResult> PreRegister(int? EID)
		{
			if (EID == null) 
			{
				return RedirectToAction("Welcome", "Home");
			}
		
			await Order.OrderUpdate(_db, (int)EID);
            ViewBag.EID = (int)EID;

			return View();
		}

		//
		// POST: Visitor/Register
		[HttpPost]
		[AutoValidateAntiforgeryToken]
		public async Task <IActionResult> Register(Visitor model)
		{
			if (model.OrderNumber == null || model.EID == 0) 
			{
				return RedirectToAction("Welcome", "Home");
			}

			Order order = await _db.Orders.SingleOrDefaultAsync(x => x.Number.ToString() == model.OrderNumber);

			if (order != null)
            {
				if (order.Full)
				{
					ViewBag.Message = "All visitors with this ORDER number were registered. Please use another ORDER number or purchase a ticket.";
					ViewBag.EID = model.EID;
					return View("PreRegister");
				}
				else if (order.Cancelled)
				{
					ViewBag.Message = "This Order was cancelled.";
					ViewBag.EID = model.EID;
					return View("PreRegister");
				}

				model.TicketType = order.TicketType;
				model.Type = order.VType;
				return View(model);
			}

            ViewBag.Message = "This ORDER number isn't correct.  Please try again.";
			ViewBag.EID = model.EID;

            return View("PreRegister");
		}

		//
		// POST: Visitor/RegisterOk
		[HttpPost]
		[AutoValidateAntiforgeryToken]
		public async Task<IActionResult> RegisterOk(Visitor model)
		{
			if (!ModelState.IsValid) 
			{
				return RedirectToAction("Menu", "Home", new {EID = model.EID});
			}

			// if dublicate found/already registered
			if (await _db.Visitors.AnyAsync(x => 
				x.OrderNumber == model.OrderNumber 
				&& x.Name == model.Name && x.Last == model.Last))
			{
				ViewBag.Message = "Record for this attendee already exist. If you have any questions please refer to one of the representative.";
				return View("Register", model);
			}

			await model.CompleteRegistration(_db);
			Event e = await _db.Events.FirstOrDefaultAsync(x => x.ID == model.EID);
			
			// add log for visitor
			model.AddLog("Registration", "Initial", true);
			e.Visitors.Add(model);

			await Order.Increment(model.OrderNumber, _db);
			await _db.SaveChangesAsync();

			return View(model);
		}
	}
}
