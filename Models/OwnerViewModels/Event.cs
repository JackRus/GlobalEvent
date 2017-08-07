using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using GlobalEvent.Data;
using GlobalEvent.Models.VisitorViewModels;
using Microsoft.EntityFrameworkCore;

namespace GlobalEvent.Models.OwnerViewModels
{
	public class Event
	{
		public int ID { get; set; }
        
		[Required]
		[Display(Name = "Name of the Event")]
		public string Name { get; set; }

		[Display(Name = "Active")]
		public bool Status { get; set; } // on/off
		public bool Archived { get; set; }

		[Required]
		[Display(Name = "Begins on")]
		public string DateStart { get; set; }

		[Required]
		[Display(Name = "Begins at")]
		public string TimeStart { get; set; }

		[Required]
		[Display(Name = "Ends on")]
		public string DateEnd { get; set; }
		
		[Required]
		[Display(Name = "Ends at")]
		public string TimeEnd { get; set; }
		public List<Visitor> Visitors { get; set; }
		public List<Ticket> Tickets { get; set; }	//types
		
		[Required]
		[Display(Name = "Eventbrite's Token")]
		public string HttpBase { get; set; } // coonection link for the source

		[Required]
		[Display(Name = "Event's Eventbrite #/ID")]
		public string EventbriteID { get; set; } // coonection link for the source

		[Required]
		[DataType(DataType.Url)]
		[Display(Name = "Event's page on Eventbrite")]
		public string TicketLink { get; set; } // link to ticket purchase page

		public int TicketsSold { get; set; }
		
		[DataType(DataType.Currency)]
		[Display(Name = "Revenue Plan, $ (optional)")]
		[DisplayFormat(DataFormatString = "{0:C}")]
		public decimal RevPlan { get; set; } // revenue planed

		[DataType(DataType.Currency)]
		[Display(Name = "Revenue Fact, $")]
		[DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = true)]
		public decimal RevFact { get; set; } // revenue fact

		[Display(Name = "Free Event")]
		public bool Free { get; set; } // if the evenT is free
		public List<Product> Products { get; set; }
        public List<Order> Orders { get; set; }     //all orders
		public List<VType> Types { get; set; } // types of visitors stuff/guests/speakers
		
		[Required]
		[StringLength(1000, MinimumLength = 10)]
		[Display(Name = "Event's description (visible to public).")]
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

		public static async Task Update (ApplicationDbContext _db, int ID)
		{
			Event e = await _db.Events
				.Include(x => x.Tickets)
				.Include(x => x.Types)
				.Include(x => x.Products)
                .Include(x => x.Orders)
				.FirstOrDefaultAsync(x => x.ID == ID);

			//TODO
		}
	}
}