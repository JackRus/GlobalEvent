using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using GlobalEvent.Models.AdminViewModels;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace GlobalEvent.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        [Required]
		[Display(Name = "First Name")]
        public string FirstName { get; set; }
        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
       
        // Determines the access level for the user: owner, manager, admin
        [Required]
        public string Level { get; set; }
    }
}
