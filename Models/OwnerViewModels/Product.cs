using System;
using System.Collections.Generic;
using GlobalEvent.Models.VisitorViewModels;

namespace GlobalEvent.Models.OwnerViewModels
{
	public class Product
	{
		public bool Free { get; set; }
		public int ID { get; set; }
        public string Name { get; set; }
		public bool Status { get; set; } // on/off
		public string DateStart { get; set; }
		public string TimeStart { get; set; }
		public string DateEnd { get; set; }
		public string TimeEnd { get; set; }
		public string Visitors { get; set; }
		public string TTypes { get; set; }	//types
		public int Capacity { get; set; }
		public int EID { get; set; }
		public string Description { get; set; }

		public Product()
		{
			this.Free = true;
			this.Status = false;
			this.Capacity = 0;
		}
	}
}