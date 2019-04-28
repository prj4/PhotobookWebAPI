using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
using PhotoBook.Repository.EventRepository;
using PhotoBook.Repository.GuestRepository;
using PhotoBook.Repository.HostRepository;
using PhotoBook.Repository.PictureRepository;
using PhotoBookDatabase.Data;
using Swashbuckle.AspNetCore.Swagger;

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


            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info
                {
                    Version = "v1",
                    Title = "Photobook API",
                    Description = "Describes the web API for the Photobook system",
                  
                });

                
                // Set the comments path for the Swagger JSON and UI.
               // var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlFile = "Comments" + ".xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
                
            });

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
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+/; ";
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 1;
            }).AddEntityFrameworkStores<AppDBContext>().AddDefaultTokenProviders();
            
            services.AddScoped<IHostRepository, HostRepository>(serviceProvider =>
                {
                    return new HostRepository(new PhotoBookDbContext(new DbContextOptionsBuilder<PhotoBookDbContext>()
                        .UseSqlServer(Configuration.GetConnectionString("RemoteConnection"))
                        .Options));
                });
            services.AddScoped<IGuestRepository, GuestRepository>(serviceProvider =>
                {
                    return new GuestRepository(new PhotoBookDbContext(new DbContextOptionsBuilder<PhotoBookDbContext>()
                        .UseSqlServer(Configuration.GetConnectionString("RemoteConnection"))
                        .Options));
                });
            services.AddScoped<IEventRepository, EventRepository>(serviceProvider =>
                {
                    return new EventRepository(new PhotoBookDbContext(new DbContextOptionsBuilder<PhotoBookDbContext>()
                        .UseSqlServer(Configuration.GetConnectionString("RemoteConnection"))
                        .Options));
                });
            services.AddScoped<IPictureRepository, PictureRepository>(serviceProvider =>
            {
                return new PictureRepository(new PhotoBookDbContext(new DbContextOptionsBuilder<PhotoBookDbContext>()
                    .UseSqlServer(Configuration.GetConnectionString("RemoteConnection"))
                    .Options));
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

            app.UseCors(builder=>builder.WithOrigins("*").AllowAnyMethod().AllowAnyHeader());

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), 
            // specifying the Swagger JSON endpoint.

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Photobook webAPI V1");
                
            });

            app.UseAuthentication();
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseHttpsRedirection();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "Host",
                    template: "{controller}/{action=Index}/{id?}");
            });

        }
    }
}
