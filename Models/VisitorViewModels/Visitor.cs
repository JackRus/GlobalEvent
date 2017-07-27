using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using GlobalEvent.Data;
using GlobalEvent.Models.AdminViewModels;

namespace GlobalEvent.Models.VisitorViewModels
{
	public class Visitor
	{
		public int ID { get; set; }
        [Display(Name = "Attendee")]
		public string Type { get; set; } // type of the visitor Guest// exibitor//stuff

		[Required]
		[Display(Name = "First Name")]
		[RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Contains non Alphabetic characters.")]
		public string Name { get; set; }

		[Required]
		[Display(Name = "Last Name")]
		[RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Contains non Alphabetic characters.")]
		public string Last { get; set; }

		[Display(Name = "Order Number")]
		public string OrderNumber { get; set; }
		[Display(Name = "Registration Number")]
		public string RegistrationNumber { get; set; }

		[Required]
		[EmailAddress]
		[Display(Name = "Work Email")]
		public string Email { get; set; }

		[Required(ErrorMessage = "Phone number is required.")]
		[Phone]
		[RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Not a valid Phone number")]
		[Display(Name = "Work/Cell Phone")]
		public string Phone { get; set; }

		[DataType(DataType.PhoneNumber)]
		[Display(Name = "Ext. (optional)")]
		public string Extention { get; set; }

		[Required]
		public string Occupation { get; set; }

		[Required]
		public string Company { get; set; }
		public int EID { get; set; }
		public string TicketType { get; set; }

		// non mandatory
		public bool GroupOwner { get; set; }
		public bool CheckIned { get; set; }
		public bool Registered { get; set; }
		public List<Request> Requests { get; set; }
		public List<VisitorLog> Logs { get; set; }
		public List<Note> Notes { get; set; }
		public string RegDate { get; set; }
		public string RegTime { get; set; }
		public string CheckDate { get; set; }
		public string CheckTime { get; set; }
		public bool Blocked { get; set; }
		public string BlockReason { get; set; }
		public bool Deleted { get; set; }

		public Visitor()
		{
			this.GroupOwner = false;
			this.CheckIned = false;
			this.Registered = false;
            this.RegDate = DateTime.Now.ToString("yyyy/MM/dd");
			this.RegTime = DateTime.Now.ToString("HH:mm:ss");
            this.Requests = new List<Request>();
            this.Notes = new List<Note>();
            this.Logs = new List<VisitorLog>();
			this.Type = "Guest";
			this.Blocked = false;
			this.Deleted = false;
		}

		public static void CompleteRegistration (Visitor v, ApplicationDbContext _db)
		{
			// generates random REGISTRATION number/ exclude duplicates
			string rand;
		    do
			{
				rand = new Random().Next(1000000000,2140999999).ToString();
			} while (_db.Visitors.Any(x => x.RegistrationNumber == rand));
			
			v.RegistrationNumber = rand;
            v.Registered = true;
			v.RegDate = DateTime.Now.ToString("yyyy-MM-dd");
			v.RegTime = DateTime.Now.ToString("HH:mm");
			v.Logs.Add( new VisitorLog("registration", "initial") { TimeEnd = DateTime.Now.ToString("HH:mm")});
		}
	}
}