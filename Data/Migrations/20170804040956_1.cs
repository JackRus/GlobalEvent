using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GlobalEvent.Data.Migrations
{
    public partial class _1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AspNetUserRoles_UserId",
                table: "AspNetUserRoles");

            migrationBuilder.DropIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles");

            migrationBuilder.CreateTable(
                name: "Changes",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Company = table.Column<string>(nullable: true),
                    Email = table.Column<string>(nullable: true),
                    Extention = table.Column<string>(nullable: true),
                    Last = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    Occupation = table.Column<string>(nullable: true),
                    ParentID = table.Column<int>(nullable: false),
                    Phone = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Changes", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Issues",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AdminID = table.Column<int>(nullable: false),
                    AdminName = table.Column<string>(nullable: true),
                    Body = table.Column<string>(nullable: true),
                    Date = table.Column<string>(nullable: true),
                    Solved = table.Column<bool>(nullable: false),
                    Status = table.Column<string>(nullable: true),
                    Time = table.Column<string>(nullable: true),
                    Type = table.Column<string>(nullable: true),
                    VID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Issues", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Logs",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Action = table.Column<string>(nullable: true),
                    AdminID = table.Column<int>(nullable: false),
                    AdminName = table.Column<string>(nullable: true),
                    Date = table.Column<string>(nullable: true),
                    Time = table.Column<string>(nullable: true),
                    Type = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Logs", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "ToDos",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Comments = table.Column<string>(nullable: true),
                    Deadline = table.Column<string>(nullable: false),
                    Done = table.Column<bool>(nullable: false),
                    EID = table.Column<int>(nullable: false),
                    Task = table.Column<string>(maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ToDos", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Archived = table.Column<bool>(nullable: false),
                    DateEnd = table.Column<string>(nullable: false),
                    DateStart = table.Column<string>(nullable: false),
                    Description = table.Column<string>(maxLength: 1000, nullable: false),
                    EventbriteID = table.Column<string>(nullable: false),
                    Free = table.Column<bool>(nullable: false),
                    HttpBase = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    RevFact = table.Column<decimal>(nullable: false),
                    RevPlan = table.Column<decimal>(nullable: false),
                    Status = table.Column<bool>(nullable: false),
                    TicketLink = table.Column<string>(nullable: false),
                    TicketsSold = table.Column<int>(nullable: false),
                    TimeEnd = table.Column<string>(nullable: false),
                    TimeStart = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Capacity = table.Column<int>(nullable: false),
                    DateEnd = table.Column<string>(nullable: true),
                    DateStart = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    EID = table.Column<int>(nullable: false),
                    EventID = table.Column<int>(nullable: true),
                    Free = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Status = table.Column<bool>(nullable: false),
                    TTypes = table.Column<string>(nullable: true),
                    TimeEnd = table.Column<string>(nullable: true),
                    TimeStart = table.Column<string>(nullable: true),
                    Visitors = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Products_Events_EventID",
                        column: x => x.EventID,
                        principalTable: "Events",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tickets",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Description = table.Column<string>(nullable: true),
                    EID = table.Column<int>(nullable: false),
                    EventID = table.Column<int>(nullable: true),
                    Limit = table.Column<int>(nullable: false),
                    Price = table.Column<decimal>(nullable: false),
                    Products = table.Column<string>(nullable: true),
                    Type = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tickets", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Tickets_Events_EventID",
                        column: x => x.EventID,
                        principalTable: "Events",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Types",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Description = table.Column<string>(nullable: true),
                    EID = table.Column<int>(nullable: false),
                    EventID = table.Column<int>(nullable: true),
                    Free = table.Column<bool>(nullable: false),
                    Limited = table.Column<bool>(nullable: false),
                    MaxLimit = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Types", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Types_Events_EventID",
                        column: x => x.EventID,
                        principalTable: "Events",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Amount = table.Column<int>(nullable: false),
                    Cancelled = table.Column<bool>(nullable: false),
                    CheckedIn = table.Column<int>(nullable: false),
                    Date = table.Column<string>(nullable: true),
                    EID = table.Column<int>(nullable: false),
                    EventID = table.Column<int>(nullable: true),
                    Full = table.Column<bool>(nullable: false),
                    Number = table.Column<int>(nullable: false),
                    OwnerEmail = table.Column<string>(nullable: true),
                    OwnerName = table.Column<string>(nullable: true),
                    OwnerPhone = table.Column<string>(nullable: true),
                    TicketType = table.Column<string>(nullable: true),
                    Time = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Orders_Events_EventID",
                        column: x => x.EventID,
                        principalTable: "Events",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Visitors",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BlockReason = table.Column<string>(nullable: true),
                    Blocked = table.Column<bool>(nullable: false),
                    CheckDate = table.Column<string>(nullable: true),
                    CheckIned = table.Column<bool>(nullable: false),
                    CheckTime = table.Column<string>(nullable: true),
                    Company = table.Column<string>(nullable: false),
                    Deleted = table.Column<bool>(nullable: false),
                    EID = table.Column<int>(nullable: false),
                    Email = table.Column<string>(nullable: false),
                    EventID = table.Column<int>(nullable: true),
                    Extention = table.Column<string>(nullable: true),
                    GroupOwner = table.Column<bool>(nullable: false),
                    Last = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Occupation = table.Column<string>(nullable: false),
                    OrderNumber = table.Column<string>(nullable: false),
                    Phone = table.Column<string>(nullable: false),
                    RegDate = table.Column<string>(nullable: true),
                    RegTime = table.Column<string>(nullable: true),
                    Registered = table.Column<bool>(nullable: false),
                    RegistrationNumber = table.Column<string>(nullable: false),
                    TicketType = table.Column<string>(nullable: true),
                    Type = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Visitors", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Visitors_Events_EventID",
                        column: x => x.EventID,
                        principalTable: "Events",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Notes",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AdminID = table.Column<int>(nullable: false),
                    AdminName = table.Column<string>(nullable: true),
                    Body = table.Column<string>(nullable: true),
                    Date = table.Column<string>(nullable: true),
                    Status = table.Column<bool>(nullable: false),
                    Time = table.Column<string>(nullable: true),
                    VID = table.Column<int>(nullable: false),
                    VisitorID = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notes", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Notes_Visitors_VisitorID",
                        column: x => x.VisitorID,
                        principalTable: "Visitors",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Requests",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AdminID = table.Column<int>(nullable: false),
                    AdminName = table.Column<string>(nullable: true),
                    Body = table.Column<string>(nullable: true),
                    Date = table.Column<string>(nullable: true),
                    Important = table.Column<bool>(nullable: false),
                    Solved = table.Column<bool>(nullable: false),
                    Time = table.Column<string>(nullable: true),
                    VID = table.Column<int>(nullable: false),
                    VType = table.Column<string>(nullable: true),
                    VisitorID = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Requests", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Requests_Visitors_VisitorID",
                        column: x => x.VisitorID,
                        principalTable: "Visitors",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VLogs",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Action = table.Column<string>(nullable: true),
                    AfterID = table.Column<int>(nullable: true),
                    BeforeID = table.Column<int>(nullable: true),
                    Date = table.Column<string>(nullable: true),
                    TimeBegin = table.Column<string>(nullable: true),
                    TimeEnd = table.Column<string>(nullable: true),
                    Type = table.Column<string>(nullable: true),
                    VID = table.Column<int>(nullable: false),
                    VisitorID = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VLogs", x => x.ID);
                    table.ForeignKey(
                        name: "FK_VLogs_Changes_AfterID",
                        column: x => x.AfterID,
                        principalTable: "Changes",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VLogs_Changes_BeforeID",
                        column: x => x.BeforeID,
                        principalTable: "Changes",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VLogs_Visitors_VisitorID",
                        column: x => x.VisitorID,
                        principalTable: "Visitors",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notes_VisitorID",
                table: "Notes",
                column: "VisitorID");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_VisitorID",
                table: "Requests",
                column: "VisitorID");

            migrationBuilder.CreateIndex(
                name: "IX_VLogs_AfterID",
                table: "VLogs",
                column: "AfterID");

            migrationBuilder.CreateIndex(
                name: "IX_VLogs_BeforeID",
                table: "VLogs",
                column: "BeforeID");

            migrationBuilder.CreateIndex(
                name: "IX_VLogs_VisitorID",
                table: "VLogs",
                column: "VisitorID");

            migrationBuilder.CreateIndex(
                name: "IX_Products_EventID",
                table: "Products",
                column: "EventID");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_EventID",
                table: "Tickets",
                column: "EventID");

            migrationBuilder.CreateIndex(
                name: "IX_Types_EventID",
                table: "Types",
                column: "EventID");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_EventID",
                table: "Orders",
                column: "EventID");

            migrationBuilder.CreateIndex(
                name: "IX_Visitors_EventID",
                table: "Visitors",
                column: "EventID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Issues");

            migrationBuilder.DropTable(
                name: "Logs");

            migrationBuilder.DropTable(
                name: "Notes");

            migrationBuilder.DropTable(
                name: "Requests");

            migrationBuilder.DropTable(
                name: "ToDos");

            migrationBuilder.DropTable(
                name: "VLogs");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Tickets");

            migrationBuilder.DropTable(
                name: "Types");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Changes");

            migrationBuilder.DropTable(
                name: "Visitors");

            migrationBuilder.DropTable(
                name: "Events");

            migrationBuilder.DropIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_UserId",
                table: "AspNetUserRoles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName");
        }
    }
}
