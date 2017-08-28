using System;

namespace GlobalEvent.Models.AdminViewModels
{
    public class VisitorLog
    {
        public int ID { get; set; } 
        public string Type { get; set; } // type of action (ex: checkin help) ex: change, add, new, login, logout etc
        public string Action { get; set; } // description
        public string Date { get; set; }
        public string Time { get; set; } 
        public Change CurrentState { get; set; } 
        public int VID { get; set; }

        public VisitorLog ()
        {
            this.Date = DateTime.Now.ToString("yyyy-MM-dd");
            this.Time = DateTime.Now.ToString("HH:mm");
        }
    }
}