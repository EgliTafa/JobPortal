using JobPortal.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Mail;

namespace JobPortal
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
            services.AddDbContext<ApplicationDbContext>(x => x.
                UseMySql(Configuration.GetConnectionString("DefaultConnection"), new MySqlServerVersion(new Version(8, 0, 11)))
                    .ConfigureWarnings(warnings => warnings.Ignore(CoreEventId.ManyServiceProvidersCreatedWarning)));

            services.AddIdentity<User, IdentityRole>(options =>
                options.Password = new PasswordOptions
                {
                    RequireDigit = true,
                    RequiredLength = 6,
                    RequireLowercase = false,
                    RequireUppercase = false,
                    RequireNonAlphanumeric = false,
                }
            )
               .AddEntityFrameworkStores<ApplicationDbContext>();

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddControllersWithViews();

            //---Gmail
            var from = Configuration.GetSection("Email")["From"];

            var gmailSender = Configuration.GetSection("Gmail")["Sender"];
            var gmailPassword = Configuration.GetSection("Gmail")["Password"];
            var gmailPort = Convert.ToInt32(Configuration.GetSection("Gmail")["Port"]);

            services
                .AddFluentEmail(gmailSender, from)
                .AddRazorRenderer()
                .AddSmtpSender(new SmtpClient("smtp.gmail.com")
                {
                    UseDefaultCredentials = false,
                    Port = gmailPort,
                    Credentials = new NetworkCredential(gmailSender, gmailPassword),
                    EnableSsl = true,
                });

            //services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            //    .AddCookie(options =>
            //        {
            //            options.AccessDeniedPath = "/Account/Login";
            //            options.LoginPath = "/Account/Login";
            //        }
            //    );
            services.ConfigureApplicationCookie(options =>
            {
                options.AccessDeniedPath = new PathString("/Account/Login");
                options.LoginPath = new PathString("/Account/Login");
                options.SlidingExpiration = true;
            });
            services.AddMvc(options => options.EnableEndpointRouting = false).SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
//            loggerFactory.AddFile("Logs/mylog-{Date}.txt");

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
