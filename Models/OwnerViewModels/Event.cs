using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GlobalEvent.Models.VisitorViewModels;

namespace GlobalEvent.Models.OwnerViewModels
{
	public class Event
	{
		public int ID { get; set; }
        public string Name { get; set; }
		public bool Status { get; set; } // on/off
		public bool Archived { get; set; }
		public string DateStart { get; set; }
		public string TimeStart { get; set; }
		public string DateEnd { get; set; }
		public string TimeEnd { get; set; }
		public List<Visitor> Visitors { get; set; }
		public List<Ticket> Tickets { get; set; }	//types
		public string HttpBase { get; set; } // coonection link for the source
		public string TicketLink { get; set; } // link to ticket purchase page
		public int TicketsSold { get; set; }
		public int RevPlan { get; set; } // revenue planed
		public int RevFact { get; set; } // revenue fact
		public bool Free { get; set; } // if the evenT is free
		public List<Product> Products { get; set; }
        public List<Order> Orders { get; set; }     //all orders
		public List<VType> Types { get; set; } // types of visitors stuff/guests/speakers
		public string Description { get; set; }

		public Event()
		{
            this.Tickets = new List<Ticket>();
            this.Visitors = new List<Visitor>();
            this.Orders = new List<Order>();
			this.Products = new List<Product>();
			this.TicketsSold = 0;
			this.RevFact = 0;
			this.RevPlan = 0;
			this.Free = false;
			this.Types = new List<VType>();
			this.Archived = false;
		}
	}
}