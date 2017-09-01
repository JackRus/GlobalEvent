using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GlobalEvent.Models.AccountViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }


        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        
        // Determines the access level for the user: owner, manager, admin
        [Required]
        public string Level { get; set; }
        public List<SelectListItem> Levels { get; set; }

        public RegisterViewModel()
		{
			this.Levels = new List<SelectListItem> {
                new SelectListItem { Value = "Manager", Text = "Manager" },
                new SelectListItem { Value = "Admin", Text = "Admin" }
            };
		}
    
    }

    
}
