using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GlobalEvent.Models.AdminViewModels
{
    public class ToDo
    {
        public int ID { get; set; }
        
        [Required]
        [StringLength(200, MinimumLength = 5)]
        public string Task { get; set; } //description
        [Required]
        public string Deadline { get; set; }
        public bool Done { get; set; }  
        public string Comments { get; set; }
        public int EID { get; set; }

        public ToDo ()
        {
            this.Done = false;        
        }
    }
}