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
        public string Deadline { get; set; } //date

        public bool Done { get; set; }  // if complited

        public string Comments { get; set; } // any comments

        [Display(Name = "Event ID. Default value is 0.")]
        public int EID { get; set; } // can be assigned to a specific Event

        public ToDo ()
        {
            this.Done = false; 
            this.EID = 0;       
        }
    }
}