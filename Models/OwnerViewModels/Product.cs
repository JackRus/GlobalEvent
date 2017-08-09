using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GlobalEvent.Models.VisitorViewModels;

namespace GlobalEvent.Models.OwnerViewModels
{
	public class Product
	{
		public bool Free { get; set; }
		public int ID { get; set; }
        
		[Required]
		public string Name { get; set; }
		public bool Status { get; set; } // on/off
		public string DateStart { get; set; }
		public string TimeStart { get; set; }
		public string DateEnd { get; set; }
		public string TimeEnd { get; set; }
		public string Visitors { get; set; } // ??? string of VID or ?
		public string TTypes { get; set; }	//types
		
		[RegularExpression(@"^[0-9]{1,5}$", ErrorMessage = "Only digits are allowed.")]
		public int Capacity { get; set; }
		
		[Required]
		[RegularExpression(@"^[0-9]{1,5}$", ErrorMessage = "Only digits are allowed.")]
		public int EID { get; set; }
		public string Description { get; set; }
		public int Attendees { get; set; }

		public Product()
		{
			this.Free = true;
			this.Status = false;
			this.Capacity = 0;
		}
	}
}