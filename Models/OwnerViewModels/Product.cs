using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GlobalEvent.Models.VisitorViewModels;

namespace GlobalEvent.Models.OwnerViewModels
{
	public class Product
	{
		public int ID { get; set; }
        public bool Free { get; set; }
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
		public string TTypes { get; set; }	// ticket types
		
		[RegularExpression(@"^[0-9]{1,5}$", ErrorMessage = "Only digits are allowed.")]
		public int Capacity { get; set; }
		
		[Required]
		[RegularExpression(@"^[0-9]{1,5}$", ErrorMessage = "Only digits are allowed.")]
		public int EID { get; set; }
		public string Description { get; set; }
		
		// amount attended
		public int Attended { get; set; }
		public List<Visit> Visits { get; set; } // ID and status: IN or OUT
		// present phisically now
		public int CurrentAttendees { get; set; }

		public Product()
		{
			this.Free = true;
			this.Status = false;
			this.Capacity = 0;
			this.Visits = new List<Visit>();
			this.CurrentAttendees = 0;
			this.Attended = 0;
		}

		public void UpdateValues(Product edited)
		{
			this.Name = edited.Name;
			this.Capacity = edited.Capacity;
			this.Free = edited.Free;
			this.Status = edited.Status;
			this.DateStart = edited.DateStart;
			this.DateEnd = edited.DateEnd;
			this.TimeStart = edited.TimeStart;
			this.TimeEnd = edited.TimeEnd;
			this.Description = edited.Description;
		}
	}
}