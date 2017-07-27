using System.Collections.Generic;

namespace GlobalEvent.Models.AdminViewModels
{
    public class ToDo
    {
        public int ID { get; set; }
        public string Task { get; set; } //description
        public string Deadline { get; set; }
        public bool Done { get; set; }  
        public string Comments { get; set; }
    }
}