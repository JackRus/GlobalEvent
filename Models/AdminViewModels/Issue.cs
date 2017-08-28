using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GlobalEvent.Data;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GlobalEvent.Models.AdminViewModels
{
	public class Issue // any issues (tech, organizational etc.)
	{
		public int ID { get; set; }
		
		[Required]
		public string Description { get; set; } // description, text-area
		
		[Required]
		[Display(Name="Issue Type")]
		public string Type { get; set; } // issue type
		public string AdminName { get; set; }
		public string AdminID { get; set; }
		public string Date { get; set; } // created
		public string Time { get; set; }
		[Required]
		[Display(Name="Due Date")]
		public string ExpectedToBeSolved { get; set; } // aprx. date/time
		public bool Assigned { get; set; }	// if the issue is being solved
		public bool Solved { get; set; } // yes/no
	
        public Issue()
        {
            this.Solved = false;
			this.Date = DateTime.Now.ToString("yyyy-MM-dd");
			this.Time = DateTime.Now.ToString("HH:mm");
			this.Assigned = false;
			this.Solved = false;
        }

		public void Complete(ApplicationUser user)
		{
			this.AdminID = user.Id;
            this.AdminName = $"{user.FirstName} {user.LastName}";
		}

		public void InitiateDescription(int? ID, int? EID)
		{
			if (ID == null && EID == null){}    // do nothing
            else if (ID == null){
                this.Description = $"[Event ID:{EID}] => ";
            }
            else if (EID == null){
                this.Description = $"[Visitor ID:{ID}] => ";
            }
            else{
                this.Description = $"[Event ID:{EID}, Visitor ID:{ID}] => ";
            }
		}

		public static List<SelectListItem> GenerateTypes ()
		{
			return new List<SelectListItem>{
                new SelectListItem {Value = "Technical", Text = "Technical"},
                new SelectListItem {Value = "Order", Text = "Order"},
                new SelectListItem {Value = "Hardware", Text = "Hardware"},
                new SelectListItem {Value = "Visitor", Text = "Visitor"},
                new SelectListItem {Value = "Security", Text = "Security"},
                new SelectListItem {Value = "Other", Text = "Other"}
            };
		}

		public void CopyInfo(Issue i)
		{
			this.Description = i.Description;
			this.Solved = i.Solved;
			this.Type = i.Type;
			this.Assigned = i.Assigned;
			this.ExpectedToBeSolved = i.ExpectedToBeSolved;
		}

		public void UpdateValues(string Name, ApplicationDbContext db)
		{
			if (Name == "Solved")
			{
				this.Solved = !this.Solved;
			}
			else if (Name == "Assigned")
			{
				this.Assigned = !this.Assigned;
			}
			db.Issues.Update(this);
		}
    }
}