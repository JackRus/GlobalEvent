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
		public bool EventCanCreate { get; set; }	// owner, manager +
		public bool EventCanChangeStatus { get; set; } 	// owner + 
		public bool EventCanEdit { get; set; }	// owner +
		public bool EventCanDelete { get; set; }	// owner +
		public bool EventCanSeeAll { get; set; }	// all +
		public bool EventCanSeeDetails { get; set; } // owner +
	// visitors
		public bool VisitorCanAccessAll { get; set; }
		public bool VisitorCanAccessDetails { get; set; }
		public bool VisitorCanBlock { get; set; }
		public bool VisitorCanDelete { get; set; }
		public bool VisitorCanEdit { get; set; }
	// products
		public bool ProductCanCreateEdit { get; set; } // owner, manager + 
		public bool ProductCanDelete { get; set; }	// owner, manager +
		public bool ProductCanSeeAll { get; set; }   // all +
	// tickets
		public bool TicketCanSeeAll { get; set; } // all +
		public bool TicketCanDelete { get; set; }	// +
		public bool TicketCanCreateEdit { get; set; }	// +
	// visitor types
		public bool VTypeCanCreateEdit { get; set; }    // +
		public bool VTypeCanDelete { get; set; }	// +
		public bool VTypeCanSeeAll { get; set; }   // all +
	// orders
		public bool OrderCanCreate { get; set; }	// owner, manager
		public bool OrderCanCancel { get; set; }	// manager. owner
		public bool OrderCanSeeAll { get; set; }   // all
		
	// admins
		public bool AdminCanChangePassword { get; set; } // owner. manager +
		public bool AdminCanCreate { get; set; }	// owner, manager +
		public bool AdminCanDelete { get; set; }  // owner
		public bool AdminCanEdit { get; set; } // owner, manager +
		public bool AdminCanChangeClaims { get; set; } // owner, manager +
		public bool AdminCanSeeAll { get; set; } // owner, manager +

	// owner
		public bool OwnerCanSeeDashboard { get; set; } // owner +
		public bool Owner { get; set; }   // only owner + 
		public bool OwnerCanSeeMenu { get; set; }	// MAIN MENU: manager, owner +

	// todo
		public bool TodoCanSeeAll{ get; set; }	// manager, owner +
		public bool TodoCanAdd { get; set; } // manager, owner +
		public bool TodoCanEditDelete { get; set; } // owner, manager +
	}
}