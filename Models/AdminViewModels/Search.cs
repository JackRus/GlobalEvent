using System;
using System.ComponentModel.DataAnnotations;

namespace GlobalEvent.Models.AdminViewModels
{
    public class Search
    {
        [RegularExpression(@"^[0-9]{1,10}$", ErrorMessage = "Only digits are allowed.")]
        public int ID { get; set; } 
        public string Name { get; set; }
        public string Last { get; set; }

        [RegularExpression(@"^[0-9]{1,10}$", ErrorMessage = "Only digits are allowed.")]
        public int Order { get; set; } 
        
        [RegularExpression(@"^[0-9]{1,15}$", ErrorMessage = "Only digits are allowed.")]
        public int RegNumber { get; set; } 
    }
}