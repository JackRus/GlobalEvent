using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GlobalEvent.Models.OwnerViewModels
{
	public class EditAdmin
	{
		[Required]
		[RegularExpression(@"^[0-9a-zA-z]+@[0-9a-zA-z]+.[a-zA-z]+$", ErrorMessage = "Not a valid email.")]
		[Display(Name = "Email")]
		public string Email { get; set; }

		[StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
		[DataType(DataType.Password)]
		public string Password { get; set; }

		[DataType(DataType.Password)]
		[Display(Name = "Confirm password")]
		[Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
		public string ConfirmPassword { get; set; }
		public string Id { get; set; }

		[Required]
		[Display(Name = "First Name")]
		public string FirstName { get; set; }

		[Required]
		[Display(Name = "Last Name")]
		public string LastName { get; set; }

		[Required]
		// Determines the access level for the user: owner, manager, admin
		// Doesn't affect the actual level
		public string Level { get; set; }

		public List<SelectListItem> Levels { get; set; }


		public void CopyValues(ApplicationUser u)
		{
			this.FirstName = u.FirstName;
			this.LastName = u.LastName;
			this.Level = u.Level;
			this.Email = u.Email;
			this.Id = u.Id;

		}

        public EditAdmin()
		{
			this.Levels = new List<SelectListItem> {
                new SelectListItem { Value = "Manager", Text = "Manager" },
                new SelectListItem { Value = "Admin", Text = "Admin" }
            };
		}
	}
}
