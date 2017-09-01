using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using GlobalEvent.Data;
using GlobalEvent.Models;
using GlobalEvent.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Http;

namespace GlobalEvent
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see https://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets<Startup>();
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>(identityOptions =>
            {
                // enables immediate logout, after updating the user's stat.
                identityOptions.SecurityStampValidationInterval = TimeSpan.Zero;
            })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();
                
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddAuthorization(options => {
                // EVENTS
                options.AddPolicy("Event Editor", x => {x.RequireClaim("EventCanEdit");});
                options.AddPolicy("Event Creator", x => x.RequireClaim("EventCanCreate"));
                options.AddPolicy("Event Viewer", x => x.RequireClaim("EventCanSeeDetails"));
                options.AddPolicy("Events Viewer", x => x.RequireClaim("EventCanSeeAll"));
                options.AddPolicy("Event Killer", x => x.RequireClaim("EventCanDelete"));
                options.AddPolicy("Event Activator", x => x.RequireClaim("EventCanChangeStatus"));
                // VISITORS
                options.AddPolicy("Visitors Viewer", x => x.RequireClaim("VisitorCanAccessAll"));
                options.AddPolicy("Visitor Details", x => x.RequireClaim("VisitorCanAccessDetails"));
                options.AddPolicy("Visitor Blocker", x => x.RequireClaim("VisitorCanBlock"));
                options.AddPolicy("Visitor Killer", x => x.RequireClaim("VisitorCanDelete"));
                options.AddPolicy("Visitor Editor", x => x.RequireClaim("VisitorCanEdit"));
                // ADMIN
                options.AddPolicy("Admins Viewer", x => x.RequireClaim("AdminCanSeeAll"));
                options.AddPolicy("Admin Editor", x => x.RequireClaim("AdminCanEdit"));
                options.AddPolicy("Admin Creator", x => x.RequireClaim("AdminCanCreate"));
                options.AddPolicy("Admin Killer", x => x.RequireClaim("AdminCanDelete")); 
                options.AddPolicy("Password Editor", x => {x.RequireClaim("AdminCanChangePassword");});
                options.AddPolicy("Claims Editor", x => x.RequireClaim("AdminCanChangeClaims"));
                // PRODUCT
                options.AddPolicy("Products Viewer", x => x.RequireClaim("ProductCanSeeAll"));
                options.AddPolicy("Product Creator", x => x.RequireClaim("ProductCanCreateEdit"));
                options.AddPolicy("Product Killer", x => x.RequireClaim("ProductCanDelete"));
                // TICKET
                options.AddPolicy("Ticket Creator", x => x.RequireClaim("TicketCanCreateEdit"));
                options.AddPolicy("Ticket Killer", x => x.RequireClaim("TicketCanDelete"));
                options.AddPolicy("Tickets Viewer", x => x.RequireClaim("TicketCanSeeAll"));
                // Visitor Types
                options.AddPolicy("VType Creator", x => x.RequireClaim("VTypeCanCreateEdit"));
                options.AddPolicy("VType Killer", x => x.RequireClaim("VTypeCanDelete"));
                options.AddPolicy("VTypes Viewer", x => x.RequireClaim("VTypeCanSeeAll"));
                // TODOs
                options.AddPolicy("Todo Viewer", x => x.RequireClaim("TodoCanSeeAll"));
                options.AddPolicy("Todo Creator", x => x.RequireClaim("TodoCanAdd"));
                options.AddPolicy("Todo EditorKiller", x => x.RequireClaim("TodoCanEditDelete"));
                // OWNER
                options.AddPolicy("Is Owner", x => x.RequireClaim("OwnerCanDoExtra"));
                options.AddPolicy("Owner's Menu", x => x.RequireClaim("OwnerCanSeeMenu"));
                options.AddPolicy("Owner's Dashboard", x => x.RequireClaim("OwnerCanSeeDashboard"));
                // orders
                options.AddPolicy("Order Creator", x => x.RequireClaim("OrderCanCreate"));
                options.AddPolicy("Order Canceler", x => x.RequireClaim("OrderCanCancel"));
                options.AddPolicy("Order Viewer", x => x.RequireClaim("OrderCanSeeAll"));
            });

            services.AddMvc();

            // Configure Identity
            services.Configure<IdentityOptions>(options =>
            {
                // Password settings
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
                options.Lockout.MaxFailedAccessAttempts = 5;

                // Cookie settings
                options.Cookies.ApplicationCookie.ExpireTimeSpan = TimeSpan.FromDays(1);
                options.Cookies.ApplicationCookie.LoginPath = "/Account/LogIn";
                options.Cookies.ApplicationCookie.LogoutPath = "/Account/LogOut";

                // User settings
                options.User.RequireUniqueEmail = true;
            });

            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, ApplicationDbContext _db)
        {
            // make sure the db exists
            _db.Database.EnsureCreated();
            app.UseIdentity();

            app.UseForwardedHeaders(new ForwardedHeadersOptions{
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Shared/Error");
            }

            app.UseStaticFiles();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
