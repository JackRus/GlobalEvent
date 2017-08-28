using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using GlobalEvent.Data;
using GlobalEvent.Models.OwnerViewModels;
using GlobalEvent.Models.VisitorViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GlobalEvent.Models.AdminViewModels
{
    public class EditVisitor
    {
        public int ID { get; set; }
        [Display(Name = "Visitor Type")]
		public string Type { get; set; } // type of the visitor Guest// exibitor//stuff

		[Required]
		[Display(Name = "First Name")]
		public string Name { get; set; }

		[Required]
		[Display(Name = "Last Name")]
		public string Last { get; set; }

		[Required]
		[RegularExpression(@"^[0-9a-zA-z]+@[0-9a-zA-z]+.[a-zA-z]+$", ErrorMessage = "Not a valid email.")]
		[Display(Name = "Email")]
		public string Email { get; set; }

		[Required(ErrorMessage = "Phone number is required.")]
		[RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Not a valid Phone number.")]
		[Display(Name = "Work/Cell Phone")]
		public string Phone { get; set; }

		[Display(Name = "Ext. (optional)")]
		[RegularExpression(@"^[0-9]{1,6}$", ErrorMessage = "Only digits are allowed.")]
		public string Extention { get; set; }

		[Required]
		[RegularExpression(@"^[a-zA-Z-' ]+$", ErrorMessage = "Contains non Alphabetic characters.")]
		public string Occupation { get; set; }

		[Required]
		[RegularExpression(@"^[0-9a-zA-Z-' ]+$", ErrorMessage = "Contains non Alphanumeric characters.")]
		public string Company { get; set; }

		[Display(Name = "Ticket Type")]
		public string TicketType { get; set; }
		public bool Blocked { get; set; }
		
		[Display(Name = "Block Reason")]
		public string BlockReason { get; set; }
		public bool Deleted { get; set; }

        public List<SelectListItem> Types { get; set; }
        public List<SelectListItem> Tickets { get; set; }
        public string Event { get; set; }
        
		public EditVisitor()
		{
			this.Blocked = false;
			this.Deleted = false;
            this.Tickets = new List<SelectListItem>();
            this.Types = new List<SelectListItem>();
		}

        public async Task SetValues(ApplicationDbContext db, int ID)
        {
            Visitor v = await db.Visitors.SingleOrDefaultAsync(x => x.ID == ID);
            JackLib.CopyValues(v, this);
        
            this.Event = (await db.Events.SingleOrDefaultAsync(x => x.ID == v.EID)).Name;
            Types = await VType.GenerateTypes(db, v.EID);
            Tickets = await Ticket.GenerateTypes(db, v.EID);
        }
    }
}