using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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
		public int CheckIned { get; set; }
		public int Registered { get; set; }
		public int Sold { get; set; }
	}
}