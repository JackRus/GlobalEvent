using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using GlobalEvent.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GlobalEvent.Models.AdminViewModels
{
    public class ToDo
    {
        public int ID { get; set; }
        
        [Required]
        [StringLength(200, MinimumLength = 5, ErrorMessage="Task has to be minimum 5 charecters long.")]
        public string Task { get; set; } //description

        [Required]
        public string Deadline { get; set; } // date
        public string Created { get; set; } // date

        public bool Done { get; set; }  // if complited

        public string Comments { get; set; } // any comments

        [Display(Name = "Event ID. Default value is 0.")]
        public int EID { get; set; } // can be assigned to a specific Event

        public ToDo ()
        {
            this.Done = false; 
            this.EID = 0;       
        }

        public void CopyValues(ToDo t, ApplicationDbContext db)
        {
            this.Task = t.Task;
            this.Comments = t.Comments;
            this.Done = t.Done;
            this.EID = t.EID;
            this.Deadline = t.Deadline;
            this.Created = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
            db.ToDos.Update(this);
        }

        public static async Task<List<SelectListItem>> GenerateEventList (ApplicationDbContext db)
        {
            var list = new List<SelectListItem>();
            var events = await db.Events.Where(x => !x.Archived).ToListAsync();
            events.ForEach(x => {
                list.Add( new SelectListItem {
                    Value = x.ID.ToString(),
                    Text = $"{x.Name}: {(x.Status ? "Active" : "Inactive")}"
                });
            });
            return list;
        }
    }
}