using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using GlobalEvent.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GlobalEvent.Models.OwnerViewModels
{
	public class Ticket
	{
		public Ticket()
		{
			this.Price = 0;
			this.Limit = 0;
			this.CheckIned = 0;
			this.Registered = 0;
			this.Sold = 0;
		}
		public int ID { get; set; }

		[Required]
		public string Type { get; set; }    // name: VIP, Standard, General Admission etc

		[Required]
		[DataType(DataType.Currency, ErrorMessage = "Has to be a decimal or integer value.")]
		[DisplayFormat(DataFormatString = "{0:C}")]
		public decimal Price { get; set; }

		[Display(Name = "Amount for Sale")]
		[RegularExpression(@"^[0-9]{1,6}", ErrorMessage = "Has to be a number.")]
		public int Limit { get; set; }      // max amount for sale/available

		public string Description { get; set; } // visible to public
		public int EID { get; set; }
		// amount of checked in visitors with this type of the ticket
		public int CheckIned { get; set; }
		// amount of registered in visitors with this type of the ticket
		public int Registered { get; set; }
		// amount sold
		public int Sold { get; set; }

		public static async Task<List<SelectListItem>> GenerateTypes(ApplicationDbContext db, int? EID)
        {
            var types = await db.Tickets.Where(x => x.EID == EID).ToListAsync();
            var list = new List<SelectListItem>();
            types.ForEach(x => list.Add(new SelectListItem { Value = x.Type, Text = x.Type }));

            return list;
        }
	}
}