using System;

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
        public bool SeenByAdmin { get; set; } 

        public Request()
        {
            this.Important = false;
            this.Solved = false;
            this.SeenByAdmin = false;
            this.Date = DateTime.Now.ToString("yyyy-MM-dd");
            this.Time = DateTime.Now.ToString("HH:mm");
        }
    }
}