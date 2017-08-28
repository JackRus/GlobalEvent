using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using GlobalEvent.Data;
using Microsoft.EntityFrameworkCore;

namespace GlobalEvent.Models.AdminViewModels
{
    public class Request // Visitor's claims, requests
    {
        public int ID { get; set; } 
        public string Description { get; set; } // text-area, description, details
        public string Date { get; set; }
        public string Time { get; set; }
        public bool Important { get; set; } // yes/no
        public bool Solved { get; set; } // yes/no
        public string AdminID { get; set; } // who made it
        public string AdminName { get; set; }
        public string VType { get; set; }
        public int VID { get; set; } // visitor ID
        
        [Display(Name = "Seen By Admin")]
        public bool SeenByAdmin { get; set; } 

        public Request()
        {
            this.Important = false;
            this.Solved = false;
            this.SeenByAdmin = false;
            this.Date = DateTime.Now.ToString("yyyy-MM-dd");
            this.Time = DateTime.Now.ToString("HH:mm");
        }

        public void UpdateValue(string Name, ApplicationDbContext db)
        {
            if (Name == "SEEN"){
                this.SeenByAdmin = !this.SeenByAdmin;
            }
            else if (Name == "IMPORTANT"){ 
                this.Important = !this.Important;
            }
            else if (Name == "SOLVED"){ 
                this.Solved = !this.Solved;
            }
            db.Requests.Update(this);
        }

        public async Task<int> CopyValues(ApplicationDbContext db)
        {
			
            Request rOld = await db.Requests.FirstOrDefaultAsync(x => x.ID == this.ID);
            rOld.Description = this.Description;
            rOld.SeenByAdmin = this.SeenByAdmin;
            rOld.Solved = this.Solved;
            rOld.Important = this.Important;
            db.Requests.Update(rOld);

            return rOld.VID;
		}
    }
}