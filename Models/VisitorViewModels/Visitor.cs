using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using GlobalEvent.Data;
using GlobalEvent.Models.AdminViewModels;
using Microsoft.EntityFrameworkCore;

namespace GlobalEvent.Models.VisitorViewModels
{
	public class Visitor 
	{
		public int ID { get; set; }
        [Display(Name = "Attendee")]
		public string Type { get; set; } // type of the visitor Guest// exibitor//stuff

		[Required]
		[Display(Name = "First Name")]
		[RegularExpression(@"^[a-zA-Z'- ]+$", ErrorMessage = "Contains non Alphabetic characters.")]
		public string Name { get; set; }

		[Required]
		[Display(Name = "Last Name")]
		[RegularExpression(@"^[a-zA-Z ]+$", ErrorMessage = "Contains non Alphabetic characters.")]
		public string Last { get; set; }

		[Required]
		[RegularExpression(@"^[0-9]+$", ErrorMessage = "Only digits are allowed.")]
		[Display(Name = "Order Number")]
		public string OrderNumber { get; set; }

		[RegularExpression(@"^[0-9]+$", ErrorMessage = "Only digits are allowed.")]
		[Display(Name = "Registration Number")]
		public string RegistrationNumber { get; set; }

		[Required]
		[RegularExpression(@"^[0-9a-zA-z]+@[0-9a-zA-z]+.[a-zA-z]+$", ErrorMessage = "Not a valid email.")]
		[Display(Name = "Email")]
		public string Email { get; set; }

		[Required(ErrorMessage = "Phone number is required.")]
		[RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Not a valid Phone number.")]
		[Display(Name = "Work/Cell Phone")]
		public string Phone { get; set; }

		[Display(Name = "Ext. (optional)")]
		[RegularExpression(@"^[0-9]+$", ErrorMessage = "Only digits are allowed in Extention.")]
		public string Extention { get; set; }

		[Required]
		[RegularExpression(@"^[a-zA-Z ]+$", ErrorMessage = "Contains non Alphabetic characters.")]
		public string Occupation { get; set; }

		[Required]
		[RegularExpression(@"^[0-9a-zA-Z ]+$", ErrorMessage = "Contains non Alphanumeric characters.")]
		public string Company { get; set; }
		public int EID { get; set; }
		public string TicketType { get; set; }
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
		}

		public void AddLog (string stage, string description, bool change = false)
		{
			VisitorLog vl = new VisitorLog();
			
			if (change) // if chnages need to be saved
			{
				vl.CurrentState = new Change();
				JackLib.CopyValues(this, vl.CurrentState);
				// vl.CurrentState.Name = this.Name;
				// vl.CurrentState.Last = this.Last;
				// vl.CurrentState.Company = this.Company;
				// vl.CurrentState.Phone = this.Phone;
				// vl.CurrentState.Email = this.Email;
				// vl.CurrentState.Extention = this.Extention;
				// vl.CurrentState.Occupation = this.Occupation;
			}
			
			vl.Type = stage;
            vl.Action = description;
            vl.VID = this.ID;
            vl.Date = DateTime.Now.ToString("yyyy-MM-dd");
            vl.Time = DateTime.Now.ToString("HH:mm");

			this.Logs.Add(vl);
		}

		public static async Task<List<Visitor>> Search(string ID, ApplicationDbContext db)
		{
			var list = new List<Visitor>();
			// get Active event id
            var EID = (await db.Events.SingleOrDefaultAsync(x => x.Status)).ID;
            var parsed = 0;
            int.TryParse(ID, out parsed);
            if (ID == "All Visitors")
            {
                list = await db.Visitors.Where(x => x.EID == EID).ToListAsync();
            }
            else
            {
                list = await db.Visitors
                    .Where(x => x.EID == EID && (
                        x.ID == parsed ||
                        x.Name.ToUpper() == ID.ToUpper() ||
                        x.Last.ToUpper() == ID.ToUpper() ||
                        x.Email.ToUpper() == ID.ToUpper() ||
                        x.Company.ToUpper() == ID.ToUpper() ||
                        x.Occupation.ToUpper() == ID.ToUpper() ||
                        x.OrderNumber == ID ||
                        x.RegistrationNumber == ID
                    )).ToListAsync();
                
                // partial match
                if (list == null || list.Count == 0)
                {
                    list = await db.Visitors
                        .Where(x => x.EID == EID && (
                            x.Name.ToUpper().Contains(ID.ToUpper()) ||
                            x.Last.ToUpper().Contains(ID.ToUpper()) ||
                            x.Email.ToUpper().Contains(ID.ToUpper()) ||
                            x.Company.ToUpper().Contains(ID.ToUpper()) ||
                            x.Occupation.ToUpper().Contains(ID.ToUpper()) ||
                            x.OrderNumber.Contains(ID) ||
                            x.RegistrationNumber.Contains(ID)
                        )).ToListAsync();
                }
            }		
			return list;
		}

		public async Task<Visitor> CopyValues(ApplicationDbContext db)
		{
			// get the existing visitor
			var oldV = await db.Visitors.FirstOrDefaultAsync(x => x.RegistrationNumber == this.RegistrationNumber); 

			oldV.Name = this.Name;
			oldV.Last = this.Last;
			oldV.Company = this.Company;
			oldV.Occupation = this.Occupation;
			oldV.Phone = this.Phone;
			oldV.Extention = this.Extention;
			oldV.Email = this.Email;			
			return oldV;
		}
	}
}