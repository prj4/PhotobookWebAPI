using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PhotobookWebAPI.Data;
using PhotoBook.Repository.EventGuestRepository;
using PhotoBook.Repository.EventRepository;
using PhotoBook.Repository.GuestRepository;
using PhotoBook.Repository.HostRepository;

namespace PhotobookWebAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);


            services.AddDbContext<AppDBContext>(opt =>
                opt.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddAuthorization(options =>
            {
                options.AddPolicy("IsHost",
                    policyBuilder => policyBuilder.RequireClaim("Role", "Host"));

                options.AddPolicy("IsGuest",
                    policyBuilder => policyBuilder.RequireClaim("Role", "Guest"));

                options.AddPolicy("IsAdmin",
                    policyBuilder => policyBuilder.RequireClaim("Role", "Admin"));
            });


            services.AddIdentity<AppUser, IdentityRole>(options =>
            {
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+/ ";
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 6;
            }).AddEntityFrameworkStores<AppDBContext>().AddDefaultTokenProviders();
            
            services.AddScoped<IHostRepository, HostRepository>(serviceProvider =>
                {
                    return new HostRepository(Configuration.GetConnectionString("RemoteConnection"));
                });
            services.AddScoped<IGuestRepository, GuestRepository>(serviceProvider =>
                {
                    return new GuestRepository(Configuration.GetConnectionString("RemoteConnection"));
                });
            services.AddScoped<IEventRepository, EventRepository>(serviceProvider =>
                {
                    return new EventRepository(Configuration.GetConnectionString("RemoteConnection"));
                });
            services.AddScoped<IEventGuestRepository, EventGuestRepository>(serviceProvider =>
                {
                    return new EventGuestRepository(Configuration.GetConnectionString("RemoteConnection"));
                });
                

            services.AddScoped<Microsoft.AspNetCore.Identity.IUserClaimsPrincipalFactory<AppUser>, AppClaimsPrincipalFactory>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseAuthentication();
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
