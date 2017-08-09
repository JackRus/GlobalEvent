using System.ComponentModel.DataAnnotations;

namespace GlobalEvent.Models.OwnerViewModels
{
    public class VType       // visitor type
    {
        public int ID { get; set; }
        
        [Required]
        public string Name { get; set; }
        public bool Free { get; set; } // if visitor has free admission
        public bool Limited { get; set; } // if amount is limited
        
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "Has to be a number.")]
        public int MaxLimit { get; set; } // max amount
        public int EID { get; set; }  // EventID
        public string Description { get; set; }
        public int CheckIned { get; set; }
        public int Registered { get; set; }

        public VType()
        {
            this.Limited = false;
            this.Free = false;
        }
    }
}