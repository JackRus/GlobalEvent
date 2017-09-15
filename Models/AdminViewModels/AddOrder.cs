using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using GlobalEvent.Data;
using GlobalEvent.Models.VisitorViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GlobalEvent.Models.AdminViewModels
{
	public class AddOrder
	{
		public int Number { get; set; }

		[Required]
		[Display(Name = "Quantity")]
		public int Amount { get; set; }

		[Required]
		[Display(Name = "Full Name")]
		public string OwnerName { get; set; }

		[Required]
		[Display(Name = "Email")]
		[RegularExpression(@"^[0-9a-zA-z]+@[0-9a-zA-z]+.[a-zA-z]+$", ErrorMessage = "Not a valid email.")]
		public string OwnerEmail { get; set; }

		[Required]
		[Display(Name = "Phone Number")]
		[RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Not a valid Phone number.")]
		public string OwnerPhone { get; set; }

		public int EID { get; set; }   //date / time

		[Required]
		[Display(Name = "Ticket Type")]
		public string TicketType { get; set; }

		[Required]
		[Display(Name = "Visitor Type")]
		public string VType { get; set; }

		[Required]
		[MinLength(5, ErrorMessage="Minimal length is 5 characters.")]
		[Display(Name = "Comment (reason/explanation)")]
		public string Comment { get; set; }

		public List<SelectListItem> Types { get; set; }
		public List<SelectListItem> VTypes { get; set; }

		public AddOrder()
		{
			this.Number = 0;
			this.Amount = 0;
			this.Types = new List<SelectListItem>();
			this.VTypes = new List<SelectListItem>();
		}

		public async Task CreateLists(ApplicationDbContext db)
		{
			var tickets = await db.Tickets.Where(x => x.EID == this.EID).ToListAsync();
			var visitors = await db.Types.Where(x => x.EID == this.EID).ToListAsync();

			foreach (var t in tickets)
			{
				this.Types.Add(new SelectListItem { Value = t.Type, Text = t.Type });
			}
			// select list for VTypes
			foreach (var v in visitors)
			{
				this.VTypes.Add(new SelectListItem { Value = v.Name, Text = v.Name });
			}
		}

		public void CopyInfo(Order o)
		{
			o.Amount = this.Amount;
			o.EID = this.EID;
			o.OwnerName = this.OwnerName;
			o.OwnerEmail = this.OwnerEmail;
			o.OwnerPhone = this.OwnerPhone;
			o.VType = this.VType;
			o.TicketType = this.TicketType;
		}

		public static async Task<int> GenerateNumber(ApplicationDbContext db)
		{
			int number = 0;
			do
			{
				// min and max values for Random() are big enough (safe)
				// and are less than the min value for Order Number from Eventbrite
				// this range gives us 23.3 million numbers, which we can use for OrderNumber 
				number = new Random().Next(10000000, 33333333);
			} while (await db.Orders.AnyAsync(x => x.Number == number));

			return number;
		}

		public async Task<bool> CheckDuplicates(ApplicationDbContext db, Order order)
		{
			// find dublicate
			Order orderDublicate = await db.Orders.FirstOrDefaultAsync(x =>
				x.Amount == this.Amount &&
				x.EID == this.EID &&
				x.OwnerName == this.OwnerName &&
				x.OwnerEmail == this.OwnerEmail &&
				x.OwnerPhone == this.OwnerPhone &&
				x.VType == this.VType &&
				x.TicketType == this.TicketType);

			if (orderDublicate  == null)
			{
				// if no dublicates found
				order.Number = await AddOrder.GenerateNumber(db);
				this.CopyInfo(order);
				return false;
			} 
			else
			{
				// if duplicate found
				order.Number = orderDublicate.Number;
				order.Amount = orderDublicate.Amount;
				order.VType = orderDublicate.VType;
				order.TicketType = orderDublicate.TicketType;
				return true;
			}
		}
	}
}