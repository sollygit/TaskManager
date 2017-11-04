using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskManager.Data;
using TaskManager.Models;
using TaskManager.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TaskManager.Authorization;

namespace TaskManager
{
    public class Startup
    {
        private IHostingEnvironment _hostingEnv;

        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            _hostingEnv = env;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Enable Dual Authentication 
            services.AddAuthentication()
                .AddJwtBearer(cfg =>
                {
                    cfg.RequireHttpsMetadata = false;
                    cfg.SaveToken = true;

                    cfg.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidIssuer = Configuration["Tokens:Issuer"],
                        ValidAudience = Configuration["Tokens:Issuer"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Tokens:Key"]))
                    };
                });

            // Add application services.
            services.AddTransient<DataSeeder>();
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
            
            var skipSSL = Configuration.GetValue<bool>("LocalTest:skipSSL");
            services.Configure<MvcOptions>(options =>
            {
                // Set LocalTest:skipSSL to true to skip SSL requrement in debug mode. This is useful when not using Visual Studio.
                if (_hostingEnv.IsDevelopment() && !skipSSL)
                {
                    options.Filters.Add(new RequireHttpsAttribute());
                }
            });

            services.AddMvc(config =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                config.Filters.Add(new AuthorizeFilter(policy));
            });

            // Contact Authorization handlers.
            services.AddScoped<IAuthorizationHandler, IsOwnerAuthorizationHandler>();
            services.AddSingleton<IAuthorizationHandler, AdministratorsAuthorizationHandler>();
            services.AddSingleton<IAuthorizationHandler, ManagerAuthorizationHandler>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, DataSeeder seeder)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            // dotnet user-secrets set SeedUserPW <pw>
            var testUserPw = Configuration["SeedUserPW"];

            if (String.IsNullOrEmpty(testUserPw))
            {
                throw new Exception("Use secrets manager to set SeedUserPW \n" +
                    "dotnet user-secrets set SeedUserPW <pw>");
            }

            seeder.SeedAsync(testUserPw).Wait();
        }
    }
}
