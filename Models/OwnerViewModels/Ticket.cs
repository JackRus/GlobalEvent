using System.Collections.Generic;

namespace GlobalEvent.Models.OwnerViewModels
{
	public class Ticket
	{
		public int ID { get; set; }
		public string Type { get; set; }    // name: VIP, Standard, General Admission etc
		public int Price { get; set; }
		public int Limit { get; set; }      // max amount for sale/available
		public string Description { get; set; } // visible to public
		public int EID { get; set; }
		public string Products { get; set; }
	}
}