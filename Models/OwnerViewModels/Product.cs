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
		
		[Display(Name="Begins on")]
		public string DateStart { get; set; }
		
		[Display(Name="Begins at")]
		public string TimeStart { get; set; }
		
		[Display(Name="Ends on")]
		public string DateEnd { get; set; }
		
		[Display(Name="Ends at")]
		public string TimeEnd { get; set; }
		public string Visitors { get; set; } // ??? string of VID or ?
		
		// TODO
		//public List<Tuple<int, bool>> VIDs { get; set; } // ID and status: IN or OUT

		public string TTypes { get; set; }	// ticket types
		
		[RegularExpression(@"^[0-9]{1,5}$", ErrorMessage = "Only digits are allowed.")]
		public int Capacity { get; set; }
		
		[Required]
		[RegularExpression(@"^[0-9]{1,5}$", ErrorMessage = "Only digits are allowed.")]
		public int EID { get; set; }
		public string Description { get; set; }
		
		// amount attended
		public int Attendees { get; set; }

		public Product()
		{
			this.Free = true;
			this.Status = false;
			this.Capacity = 0;
		}
	}
}