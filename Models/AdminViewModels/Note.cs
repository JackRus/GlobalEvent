using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using GlobalEvent.Data;
using Microsoft.EntityFrameworkCore;

namespace GlobalEvent.Models.AdminViewModels
{
	public class Note
    {
        public int ID { get; set; } 
        public string Description { get; set; } // description, details
        public string Date { get; set; }
        public string Time { get; set; }
        public bool Important { get; set; } // important or not
        public string AdminID { get; set; } // who made it
        public string AdminName { get; set; }
        public int VID { get; set; }
        
        [Display(Name = "Seen By Admin")]
        public bool SeenByAdmin { get; set; }

        public Note()
        {
            this.Important = false;
            this.SeenByAdmin = true;
            this.Date = DateTime.Now.ToString("yyyy-MM-dd");
            this.Time = DateTime.Now.ToString("HH:mm");
            this.Important = false;
            this.SeenByAdmin = false;
        }

        public async Task<int> CopyValues(ApplicationDbContext db)
        {
            Note nOld = await db.Notes.SingleOrDefaultAsync(x => x.ID == this.ID);
            nOld.Description = this.Description;
            nOld.SeenByAdmin = this.SeenByAdmin;
            nOld.Important = this.Important;
            db.Notes.Update(nOld);
            return nOld.VID;
        } 

        public void UpdateValues(string Name, ApplicationDbContext db)
        {
            if (Name == "SEEN")
            {
                this.SeenByAdmin = !this.SeenByAdmin;
            }
            else if (Name == "IMPORTANT")
            {
                this.Important = !this.Important;
            }
            db.Notes.Update(this);
        }
    }
}