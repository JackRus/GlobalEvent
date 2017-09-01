using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using GlobalEvent.Data;

namespace GlobalEvent.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.2");

            modelBuilder.Entity("GlobalEvent.Models.AdminViewModels.Change", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Company");

                    b.Property<string>("Email");

                    b.Property<string>("Extention");

                    b.Property<string>("Last");

                    b.Property<string>("Name");

                    b.Property<string>("Occupation");

                    b.Property<string>("Phone");

                    b.HasKey("ID");

                    b.ToTable("Changes");
                });

            modelBuilder.Entity("GlobalEvent.Models.AdminViewModels.Issue", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AdminID");

                    b.Property<string>("AdminName");

                    b.Property<bool>("Assigned");

                    b.Property<string>("Date");

                    b.Property<string>("Description")
                        .IsRequired();

                    b.Property<string>("ExpectedToBeSolved")
                        .IsRequired();

                    b.Property<bool>("Solved");

                    b.Property<string>("Time");

                    b.Property<string>("Type")
                        .IsRequired();

                    b.HasKey("ID");

                    b.ToTable("Issues");
                });

            modelBuilder.Entity("GlobalEvent.Models.AdminViewModels.Log", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AdminID");

                    b.Property<string>("AdminName");

                    b.Property<string>("Date");

                    b.Property<string>("Description");

                    b.Property<string>("Time");

                    b.Property<string>("Type");

                    b.HasKey("ID");

                    b.ToTable("Logs");
                });

            modelBuilder.Entity("GlobalEvent.Models.AdminViewModels.Note", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AdminID");

                    b.Property<string>("AdminName");

                    b.Property<string>("Date");

                    b.Property<string>("Description");

                    b.Property<bool>("Important");

                    b.Property<bool>("SeenByAdmin");

                    b.Property<string>("Time");

                    b.Property<int>("VID");

                    b.Property<int?>("VisitorID");

                    b.HasKey("ID");

                    b.HasIndex("VisitorID");

                    b.ToTable("Notes");
                });

            modelBuilder.Entity("GlobalEvent.Models.AdminViewModels.Request", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AdminID");

                    b.Property<string>("AdminName");

                    b.Property<string>("Date");

                    b.Property<string>("Description");

                    b.Property<bool>("Important");

                    b.Property<bool>("SeenByAdmin");

                    b.Property<bool>("Solved");

                    b.Property<string>("Time");

                    b.Property<int>("VID");

                    b.Property<string>("VType");

                    b.Property<int?>("VisitorID");

                    b.HasKey("ID");

                    b.HasIndex("VisitorID");

                    b.ToTable("Requests");
                });

            modelBuilder.Entity("GlobalEvent.Models.AdminViewModels.ToDo", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Comments");

                    b.Property<string>("Created");

                    b.Property<string>("Deadline")
                        .IsRequired();

                    b.Property<bool>("Done");

                    b.Property<int>("EID");

                    b.Property<string>("Task")
                        .IsRequired()
                        .HasMaxLength(200);

                    b.HasKey("ID");

                    b.ToTable("ToDos");
                });

            modelBuilder.Entity("GlobalEvent.Models.AdminViewModels.VisitorLog", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Action");

                    b.Property<int?>("CurrentStateID");

                    b.Property<string>("Date");

                    b.Property<string>("Time");

                    b.Property<string>("Type");

                    b.Property<int>("VID");

                    b.Property<int?>("VisitorID");

                    b.HasKey("ID");

                    b.HasIndex("CurrentStateID");

                    b.HasIndex("VisitorID");

                    b.ToTable("VLogs");
                });

            modelBuilder.Entity("GlobalEvent.Models.ApplicationUser", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AccessFailedCount");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Email")
                        .HasMaxLength(256);

                    b.Property<bool>("EmailConfirmed");

                    b.Property<string>("FirstName")
                        .IsRequired();

                    b.Property<string>("LastName")
                        .IsRequired();

                    b.Property<string>("Level")
                        .IsRequired();

                    b.Property<bool>("LockoutEnabled");

                    b.Property<DateTimeOffset?>("LockoutEnd");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256);

                    b.Property<string>("PasswordHash");

                    b.Property<string>("PhoneNumber");

                    b.Property<bool>("PhoneNumberConfirmed");

                    b.Property<string>("SecurityStamp");

                    b.Property<bool>("TwoFactorEnabled");

                    b.Property<string>("UserName")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasName("UserNameIndex");

                    b.ToTable("AspNetUsers");
                });

            modelBuilder.Entity("GlobalEvent.Models.OwnerViewModels.Event", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("Archived");

                    b.Property<string>("DateEnd")
                        .IsRequired();

                    b.Property<string>("DateStart")
                        .IsRequired();

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(1000);

                    b.Property<string>("EventbriteID")
                        .IsRequired();

                    b.Property<bool>("Free");

                    b.Property<string>("HttpBase")
                        .IsRequired();

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<decimal>("RevFact");

                    b.Property<decimal>("RevPlan");

                    b.Property<bool>("Status");

                    b.Property<string>("TicketLink")
                        .IsRequired();

                    b.Property<int>("TicketsSold");

                    b.Property<string>("TimeEnd")
                        .IsRequired();

                    b.Property<string>("TimeStart")
                        .IsRequired();

                    b.HasKey("ID");

                    b.ToTable("Events");
                });

            modelBuilder.Entity("GlobalEvent.Models.OwnerViewModels.Product", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("Attended");

                    b.Property<int>("Capacity");

                    b.Property<int>("CurrentAttendees");

                    b.Property<string>("DateEnd");

                    b.Property<string>("DateStart");

                    b.Property<string>("Description");

                    b.Property<int>("EID");

                    b.Property<int?>("EventID");

                    b.Property<bool>("Free");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<bool>("Status");

                    b.Property<string>("TTypes");

                    b.Property<string>("TimeEnd");

                    b.Property<string>("TimeStart");

                    b.HasKey("ID");

                    b.HasIndex("EventID");

                    b.ToTable("Products");
                });

            modelBuilder.Entity("GlobalEvent.Models.OwnerViewModels.Ticket", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("CheckIned");

                    b.Property<string>("Description");

                    b.Property<int>("EID");

                    b.Property<int?>("EventID");

                    b.Property<int>("Limit");

                    b.Property<decimal>("Price");

                    b.Property<int>("Registered");

                    b.Property<int>("Sold");

                    b.Property<string>("Type")
                        .IsRequired();

                    b.HasKey("ID");

                    b.HasIndex("EventID");

                    b.ToTable("Tickets");
                });

            modelBuilder.Entity("GlobalEvent.Models.OwnerViewModels.Visit", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("Present");

                    b.Property<int?>("ProductID");

                    b.Property<string>("Time");

                    b.Property<int>("VID");

                    b.HasKey("ID");

                    b.HasIndex("ProductID");

                    b.ToTable("Visits");
                });

            modelBuilder.Entity("GlobalEvent.Models.OwnerViewModels.VType", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("CheckIned");

                    b.Property<string>("Description");

                    b.Property<int>("EID");

                    b.Property<int?>("EventID");

                    b.Property<bool>("Free");

                    b.Property<bool>("Limited");

                    b.Property<int>("MaxLimit");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<int>("Registered");

                    b.HasKey("ID");

                    b.HasIndex("EventID");

                    b.ToTable("Types");
                });

            modelBuilder.Entity("GlobalEvent.Models.VisitorViewModels.Order", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("Amount");

                    b.Property<bool>("Cancelled");

                    b.Property<int>("CheckedIn");

                    b.Property<string>("Date");

                    b.Property<int>("EID");

                    b.Property<int?>("EventID");

                    b.Property<bool>("Full");

                    b.Property<int>("Number");

                    b.Property<string>("OwnerEmail")
                        .IsRequired();

                    b.Property<string>("OwnerName")
                        .IsRequired();

                    b.Property<string>("OwnerPhone");

                    b.Property<string>("TicketType")
                        .IsRequired();

                    b.Property<string>("Time");

                    b.Property<string>("VType")
                        .IsRequired();

                    b.HasKey("ID");

                    b.HasIndex("EventID");

                    b.ToTable("Orders");
                });

            modelBuilder.Entity("GlobalEvent.Models.VisitorViewModels.Visitor", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("BlockReason");

                    b.Property<bool>("Blocked");

                    b.Property<string>("CheckDate");

                    b.Property<bool>("CheckIned");

                    b.Property<string>("CheckTime");

                    b.Property<string>("Company")
                        .IsRequired();

                    b.Property<bool>("Deleted");

                    b.Property<int>("EID");

                    b.Property<string>("Email")
                        .IsRequired();

                    b.Property<int?>("EventID");

                    b.Property<string>("Extention");

                    b.Property<bool>("GroupOwner");

                    b.Property<string>("Last")
                        .IsRequired();

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<string>("Occupation")
                        .IsRequired();

                    b.Property<string>("OrderNumber")
                        .IsRequired();

                    b.Property<string>("Phone")
                        .IsRequired();

                    b.Property<string>("RegDate");

                    b.Property<string>("RegTime");

                    b.Property<bool>("Registered");

                    b.Property<string>("RegistrationNumber");

                    b.Property<string>("TicketType");

                    b.Property<string>("Type");

                    b.HasKey("ID");

                    b.HasIndex("EventID");

                    b.ToTable("Visitors");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Name")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasName("RoleNameIndex");

                    b.ToTable("AspNetRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("RoleId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider");

                    b.Property<string>("ProviderKey");

                    b.Property<string>("ProviderDisplayName");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("RoleId");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("LoginProvider");

                    b.Property<string>("Name");

                    b.Property<string>("Value");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens");
                });

            modelBuilder.Entity("GlobalEvent.Models.AdminViewModels.Note", b =>
                {
                    b.HasOne("GlobalEvent.Models.VisitorViewModels.Visitor")
                        .WithMany("Notes")
                        .HasForeignKey("VisitorID");
                });

            modelBuilder.Entity("GlobalEvent.Models.AdminViewModels.Request", b =>
                {
                    b.HasOne("GlobalEvent.Models.VisitorViewModels.Visitor")
                        .WithMany("Requests")
                        .HasForeignKey("VisitorID");
                });

            modelBuilder.Entity("GlobalEvent.Models.AdminViewModels.VisitorLog", b =>
                {
                    b.HasOne("GlobalEvent.Models.AdminViewModels.Change", "CurrentState")
                        .WithMany()
                        .HasForeignKey("CurrentStateID");

                    b.HasOne("GlobalEvent.Models.VisitorViewModels.Visitor")
                        .WithMany("Logs")
                        .HasForeignKey("VisitorID");
                });

            modelBuilder.Entity("GlobalEvent.Models.OwnerViewModels.Product", b =>
                {
                    b.HasOne("GlobalEvent.Models.OwnerViewModels.Event")
                        .WithMany("Products")
                        .HasForeignKey("EventID");
                });

            modelBuilder.Entity("GlobalEvent.Models.OwnerViewModels.Ticket", b =>
                {
                    b.HasOne("GlobalEvent.Models.OwnerViewModels.Event")
                        .WithMany("Tickets")
                        .HasForeignKey("EventID");
                });

            modelBuilder.Entity("GlobalEvent.Models.OwnerViewModels.Visit", b =>
                {
                    b.HasOne("GlobalEvent.Models.OwnerViewModels.Product")
                        .WithMany("Visits")
                        .HasForeignKey("ProductID");
                });

            modelBuilder.Entity("GlobalEvent.Models.OwnerViewModels.VType", b =>
                {
                    b.HasOne("GlobalEvent.Models.OwnerViewModels.Event")
                        .WithMany("Types")
                        .HasForeignKey("EventID");
                });

            modelBuilder.Entity("GlobalEvent.Models.VisitorViewModels.Order", b =>
                {
                    b.HasOne("GlobalEvent.Models.OwnerViewModels.Event")
                        .WithMany("Orders")
                        .HasForeignKey("EventID");
                });

            modelBuilder.Entity("GlobalEvent.Models.VisitorViewModels.Visitor", b =>
                {
                    b.HasOne("GlobalEvent.Models.OwnerViewModels.Event")
                        .WithMany("Visitors")
                        .HasForeignKey("EventID");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRole")
                        .WithMany("Claims")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("GlobalEvent.Models.ApplicationUser")
                        .WithMany("Claims")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("GlobalEvent.Models.ApplicationUser")
                        .WithMany("Logins")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRole")
                        .WithMany("Users")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("GlobalEvent.Models.ApplicationUser")
                        .WithMany("Roles")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
        }
    }
}
