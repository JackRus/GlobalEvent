using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using GlobalEvent.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GlobalEvent.Models.OwnerViewModels
{
    public class VType       // visitor type
    {
        public int ID { get; set; }
        
        [Required]
        public string Name { get; set; }
        public bool Free { get; set; } // if visitor has free admission
        public bool Limited { get; set; } // if amount is limited
        
        [Display(Name="Maximum Amount Allowed")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "Has to be a number.")]
        public int MaxLimit { get; set; } // max amount
        public int EID { get; set; }  // EventID
        public string Description { get; set; }
        // amount checked in
        public int CheckIned { get; set; }
        // amount registred
        public int Registered { get; set; }

        public VType()
        {
            this.Limited = false;
            this.Free = false;
        }

        public static async Task<List<SelectListItem>> GenerateTypes(ApplicationDbContext db, int? EID)
        {
            var vTypes = await db.Types.Where(x => x.EID == EID).ToListAsync();
            var list = new List<SelectListItem>();
            vTypes.ForEach(x => list.Add(new SelectListItem { Value = x.Name, Text = x.Name }));

            return list;
        }
    }
}