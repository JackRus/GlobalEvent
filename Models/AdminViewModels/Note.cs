using System;

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
    }
}