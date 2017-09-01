using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using GlobalEvent.Models;
using GlobalEvent.Models.VisitorViewModels;
using GlobalEvent.Models.AdminViewModels;
using GlobalEvent.Models.OwnerViewModels;

namespace GlobalEvent.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        
        public DbSet<Visitor> Visitors { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Request> Requests { get; set; } // customer's requests, complaints
        public DbSet<Issue> Issues { get; set; } // tech bugs, issues
        public DbSet<Log> Logs { get; set; } // all logs
        public DbSet<Note> Notes { get; set; } // admins notes about customers
        public DbSet<Change> Changes { get; set; } // personal info changes
        public DbSet<VisitorLog> VLogs { get; set; } // log for personal info change/access to account
        public DbSet<Event> Events { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<VType> Types { get; set; }
        public DbSet<ToDo> ToDos { get; set; }
        public DbSet<Visit> Visits { get; set; }
        
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}
