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

		public IActionResult CheckInConfirmation(int? EID)
		{
			if (EID == null) return RedirectToAction("Welcome", "Home");
			ViewBag.EID = (int)EID;
			return View();
		}

		[HttpGet]
		public IActionResult PreCheckIn(int? EID)
		{
			if (EID == null) return RedirectToAction("Welcome", "Home");
			return View(new Visitor(){EID = (int)EID});
		}

		[HttpPost]
		[AutoValidateAntiforgeryToken]
		public async Task<IActionResult> CheckIn(Visitor v)
		{
			if (v.RegistrationNumber == null || v.EID == 0) 
				return RedirectToAction("Menu", "Home", new { EID = v.EID});

			var newV = await _db.Visitors.FirstOrDefaultAsync(x => x.RegistrationNumber == v.RegistrationNumber);

			if (newV != null)
			{
				if (newV.CheckIned)
				{
					ViewBag.Message = "This Registration number was already used for Check In and name tag was printed. If you would like to change some information displayed on the name tag, please refer to the administrator.";
					ViewBag.EID = v.EID;
					return View("PreCheckIn", v);
				}
				ViewBag.Event = (await _db.Events.FirstOrDefaultAsync(x => x.ID == v.EID)).Name;
				return View(newV);
			}
			ViewBag.Message = "This Registration number isn't correct. Please try again.";
            return View("PreCheckIn", v);
		}

		[HttpGet]
		public async Task<IActionResult> CheckInOk (string number, int? EID)
		{
			if (number == null || EID == null) return RedirectToAction("Welcome", "Home");
			
			var v = await _db.Visitors.FirstOrDefaultAsync(x => x.RegistrationNumber == number);
			
			if (v == null || v.CheckIned)
			{
				ViewBag.Message = "Something went wrong. Please try again.";
				return RedirectToAction("Menu", "Home", new { EID = EID });
			}

			v.CheckIned = true;
			_db.Visitors.Update(v);
			await _db.SaveChangesAsync();
			return View();
		}

		[HttpGet]
		public async Task<IActionResult> Edit (string number)
		{
			if (number == null) return RedirectToAction("Welcome", "Home");
			
			Visitor v = await _db.Visitors.FirstOrDefaultAsync(x => x.RegistrationNumber == number); 

			if (v == null || v.CheckIned)
				return RedirectToAction("Welcome", "Home");
			
			return View(v);
		}

		[HttpPost]
		[AutoValidateAntiforgeryToken]
		public async Task<IActionResult> EditOk(Visitor v)
		{
			if (!ModelState.IsValid) return RedirectToAction("Menu", "Home", new {EID = v.EID});

			// get the existing visitor
			var oldV = await _db.Visitors.FirstOrDefaultAsync(x => x.RegistrationNumber == v.RegistrationNumber); 

			oldV.Name = v.Name;
			oldV.Last = v.Last;
			oldV.Company = v.Company;
			oldV.Occupation = v.Occupation;
			oldV.Phone = v.Phone;
			oldV.Extention = v.Extention;
			oldV.Email = v.Email;

			// update visitor
			_db.Visitors.Update(oldV);
			await _db.SaveChangesAsync();
			return View(oldV);
		}

		[HttpGet]
		public async Task<IActionResult> PreRegister(int? EID)
		{
			if (EID == null) return RedirectToAction("Welcome", "Home");

			// update orders, sync Eventbrite & DB
			var url = (await _db.Events.FirstOrDefaultAsync(x => x.ID == EID)).HttpBase;
			await Order.OrderUpdate(_db, url, (int)EID);

            ViewBag.EID = (int)EID;
			return View();
		}

		[HttpPost]
		[AutoValidateAntiforgeryToken]
		public async Task <IActionResult> Register(Visitor v)
		{
			if (v.OrderNumber == null || v.EID == 0) 
				return RedirectToAction("Menu", "Home", new { EID = v.EID});

			var orders = await _db.Orders.ToListAsync();
			foreach (Order o in orders)
            {
                if (o.Number.ToString() == v.OrderNumber)
                {
                    if (o.Full)
                    {
						ViewBag.Message = "All visitors with this ORDER number were registered. Please use another ORDER number or purchase a ticket.";
						ViewBag.EID = v.EID;
                        return View("PreRegister");
					}
					v.TicketType = o.TicketType;
					return View(v);
				}
			}
            ViewBag.Message = "This ORDER number isn't correct. Please try again";
			ViewBag.EID = v.EID;
            return View("PreRegister");
		}

		[HttpPost]
		[AutoValidateAntiforgeryToken]
		public async Task<IActionResult> RegisterOk(Visitor v)
		{
			if (!ModelState.IsValid) return RedirectToAction("Menu", "Home", new {EID = v.EID});

			// if dublicate found/already registered
			if (_db.Visitors.Any(x => x.OrderNumber == v.OrderNumber 
				&& x.Name == v.Name && x.Last == v.Last))
			{
				ViewBag.Message = "Record for this attendee already exist. If you have any questions please refer to one of the representative.";
				return View("Register", v);
			}
			
			// auto fill the missing info
			Visitor.CompleteRegistration(v, _db);
			Event e = await _db.Events.FirstOrDefaultAsync(x => x.ID == v.EID);
			e.Visitors.Add(v);
			
			// update order
			await Order.Increment(v.OrderNumber, _db);
			await _db.SaveChangesAsync();
			return View(v);
		}

	}
}
