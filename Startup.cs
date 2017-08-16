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

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddAuthorization(options => {
                options.AddPolicy("Event Editor", x => {x.RequireClaim("CanEditEvent");});
                options.AddPolicy("Claims Editor", x => x.RequireClaim("CanChangeClaims"));
                options.AddPolicy("Password Editor", x => {x.RequireClaim("CanChangeAdminPassword"); });
                options.AddPolicy("Event Creator", x => x.RequireClaim("CanCreateEvent"));
                options.AddPolicy("Event Viewer", x => x.RequireClaim("CanSeeEventDetails"));
                options.AddPolicy("Owner's Menu", x => x.RequireClaim("CanSeeOwnersPage"));
                options.AddPolicy("Owner's Dashboard", x => x.RequireClaim("CanSeeMainDashboard"));
                options.AddPolicy("Events Viewer", x => x.RequireClaim("CanSeeAllEvents"));
                options.AddPolicy("Event Killer", x => x.RequireClaim("CanDeleteEvent"));
                options.AddPolicy("Event Activator", x => x.RequireClaim("CanChangeEventStatus"));
                options.AddPolicy("Admins Viewer", x => x.RequireClaim("CanSeeAllAdmins"));
                options.AddPolicy("Admin Editor", x => x.RequireClaim("CanEditAdmin"));
                options.AddPolicy("Admin Creator", x => x.RequireClaim("CanCreateAdmin"));
                options.AddPolicy("Visitors Viewer", x => x.RequireClaim("CanAccessAllVisitors"));
                options.AddPolicy("Tickets Viewer", x => x.RequireClaim("CanSeeAllTickets"));
                options.AddPolicy("Products Viewer", x => x.RequireClaim("CanSeeAllProducts"));
                options.AddPolicy("VTypes Viewer", x => x.RequireClaim("CanSeeAllVTypes"));
                options.AddPolicy("Is Owner", x => x.RequireClaim("IsOwner"));
                options.AddPolicy("Product Creator", x => x.RequireClaim("CanCreateEditProduct"));
                options.AddPolicy("Product Killer", x => x.RequireClaim("CanDeleteProduct"));
                options.AddPolicy("Ticket Creator", x => x.RequireClaim("CanCreateEditTicket"));
                options.AddPolicy("Ticket Killer", x => x.RequireClaim("CanDeleteTicket"));
                options.AddPolicy("VType Creator", x => x.RequireClaim("CanCreateEditVType"));
                options.AddPolicy("VType Killer", x => x.RequireClaim("CanDeleteVType"));
                options.AddPolicy("Todo Viewer", x => x.RequireClaim("CanSeeToDoList"));
                options.AddPolicy("Todo Creator", x => x.RequireClaim("CanAddTodo"));
                options.AddPolicy("Todo EditorKiller", x => x.RequireClaim("CanEditDeleteTodo"));

            });

            services.AddMvc();

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
