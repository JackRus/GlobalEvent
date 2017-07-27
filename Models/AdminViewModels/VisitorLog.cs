using System;

namespace GlobalEvent.Models.AdminViewModels
{
    public class VisitorLog
    {
        public int ID { get; set; } 
        public string Type { get; set; } // type of action (ex: checkin help) ex: change, add, new, login, logout etc

        // registration
        // checkin
        // info-change
        // print

        public string Action { get; set; } // specific action ex: registration, checkin
        public string Date { get; set; }
        public string TimeBegin { get; set; } 
        public string TimeEnd { get; set; }
        public Change Before { get; set; }
        public Change After { get; set; }
        public int VID { get; set; }

        public VisitorLog (string type, string action)
        {
            this.Type = type;
            this.Action = action;
            this.Date = DateTime.Now.ToString("yyyy-MM-dd");
            this.TimeBegin = DateTime.Now.ToString("HH:mm");
        }

    }
}