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
using NETCore.MailKit.Extensions;
using NETCore.MailKit.Infrastructure.Internal;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

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


            services.Configure<DataProtectionTokenProviderOptions>(o =>
        o.TokenLifespan = TimeSpan.FromMinutes(30));

            services.AddIdentity<User, IdentityRole>(options =>
            {
                //options.SignIn.RequireConfirmedAccount = false;
                //options.User.RequireUniqueEmail = true;
                //options.Tokens.AuthenticatorIssuer = "JWT";
                //options.Password = new PasswordOptions
                //{
                //    RequireDigit = true,
                //    RequiredLength = 8,
                //    RequireLowercase = true,
                //    RequireUppercase = true,
                //    RequireNonAlphanumeric = false,
                //};

                //// Add this line for Email confirmation
                //options.SignIn.RequireConfirmedEmail = true;

            })//.AddDefaultTokenProviders()
            .AddEntityFrameworkStores<ApplicationDbContext>();

            var mailKitOptions = Configuration.GetSection("Email").Get<MailKitOptions>();

            services.AddMailKit(config => {
                var options = new MailKitOptions();
                config.UseMailKit(mailKitOptions);
            });

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddControllersWithViews();

            services.AddControllersWithViews(x => x.SuppressAsyncSuffixInActionNames = false)
                    .AddRazorRuntimeCompilation();

            services
                .AddFluentEmail("fromemail@test.test")
                .AddRazorRenderer()
                .AddSmtpSender("localhost", 25);

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
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IServiceProvider serviceProvider)
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
