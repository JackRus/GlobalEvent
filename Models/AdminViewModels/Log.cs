using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using GlobalEvent.Data;
using GlobalEvent.Models.AdminViewModels;

namespace GlobalEvent.Models.AdminViewModels
{
    public class Log // admins actions
    {
        public int ID { get; set; } 
        public string Type { get; set; } // type of action (ex: checkin help)
        public string Description { get; set; } // description
        public string AdminName { get; set; } 
        public string AdminID { get; set; }   
        public string Date { get; set; }
        public string Time { get; set; } 

        public Log()
        {
            this.Date = DateTime.Now.ToString("yyyy-MM-dd");
            this.Time = DateTime.Now.ToString("HH:mm");
        }
    }
}