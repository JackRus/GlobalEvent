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
		public bool CanCreateEvent { get; set; }
		public bool CanEditEvent { get; set; }
		public bool CanDeleteEvent { get; set; }
	// visitors
		public bool CanAccessAllVisitors { get; set; }
		public bool CanAccessVisitorDetails { get; set; }
		public bool CanBlockVisitor { get; set; }
		public bool CanDeleteVisitor { get; set; }
		public bool CanEditVisitor { get; set; }
	// // products
		public bool CanCreateProduct { get; set; }
		public bool CanEditProduct { get; set; }
		public bool CanDeleteProduc { get; set; }
		public bool CanChangeProductTickets { get; set; }
	// // visitor types
	// // orders
	// // admins
		public bool CanChangeAdminPassword { get; set; }
		public bool CanCreateAdmin { get; set; }
		public bool CanDeleteAdm { get; set; }
	}
}