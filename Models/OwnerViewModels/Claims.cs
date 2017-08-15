using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using GlobalEvent.Models.VisitorViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GlobalEvent.Models.OwnerViewModels
{
	public class Claims
	{
	// events
		public bool CanCreateEvent { get; set; }	// owner, manager +
		public bool CanChangeEventStatus { get; set; } 	// owner + 
		public bool CanEditEvent { get; set; }	// owner +
		public bool CanDeleteEvent { get; set; }	// owner +
		public bool CanSeeAllEvents { get; set; }	// all +
		public bool CanSeeEventDetails { get; set; } // owner +
	// visitors
		public bool CanAccessAllVisitors { get; set; }
		public bool CanAccessVisitorDetails { get; set; }
		public bool CanBlockVisitor { get; set; }
		public bool CanDeleteVisitor { get; set; }
		public bool CanEditVisitor { get; set; }
	// products
		public bool CanCreateProduct { get; set; }
		public bool CanEditProduct { get; set; }
		public bool CanDeleteProduct { get; set; }
		public bool CanChangeProductTickets { get; set; }
	// visitor types
		public bool CanCreateVType { get; set; }
		public bool CanEditVType { get; set; }
		public bool CanDeleteVType { get; set; }
	// orders
		public bool CanCreateOrder { get; set; }	// owner, manager
		public bool CanCancelOrder { get; set; }	// manager. owner
		public bool CanSeeAllOrders { get; set; }   // all
		
	// admins
		public bool CanChangeAdminPassword { get; set; } // owner. manager +
		public bool CanCreateAdmin { get; set; }	// owner, manager +
		public bool CanDeleteAdmin { get; set; }  // TODO
		public bool CanEditAdmin { get; set; } // owner, manager +
		public bool CanChangeClaims { get; set; } // owner, manager +
		public bool CanAddAdmin { get; set; }	// owner, manager +
		public bool CanSeeAllAdmins { get; set; } // owner, manager +

	// owner
		public bool CanSeeMainDashboard { get; set; } // owner +
		public bool CanSeeOwnersPage { get; set; }	// MAIN MENU: manager, owner +

	// todo
		public bool CanSeeToDoList { get; set; }	// manager, owner
		public bool CanAddTodo { get; set; }
		public bool CanEditDeleteTodo { get; set; }
	}
}