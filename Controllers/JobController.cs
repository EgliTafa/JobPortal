using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JobPortal.Models;
using JobPortal.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using JobPortal.ViewModels.Home;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using FluentEmail.Core;
using FluentEmail.Smtp;
using System.Net.Mail;
using System.Text;
using FluentEmail.Razor;
using System.Net;

namespace JobPortal.Controllers
{
    public class JobController : Controller
    {
        private readonly IHostingEnvironment hostingEnvironment;
        private readonly ILogger _logger;
        private readonly ApplicationDbContext _context;
        private UserManager<User> _userManager;

        public JobController(ApplicationDbContext context, UserManager<User> userManager, ILogger<JobController> logger, IHostingEnvironment environment)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
            hostingEnvironment = environment;
        }

        [Route("jobs")]
        public IActionResult Index()
        {


            var jobs = _context.Jobs.ToList();

            return View(jobs);
        }

        [Route("jobs/create")]
        [Authorize(Roles = "Employer")]
        public IActionResult Create()
        {
            return View();
        }

        [Route("jobs/save")]
        [Authorize(Roles = "Employer")]
        [HttpPost]
        public async Task<IActionResult> Save([Bind("User","Title","Description","Website","Location","Type","CompanyName","CompanyDescription", "Job", "CVPath", "CreatedAt","Salary","Category","LastDate","posterUrl")]
                                                Job model)
        {
            if(ModelState.IsValid)
            {
                TempData["type"] = "success";
                TempData["message"] = "Job posted successfully";
                //_logger.LogInformation(model.ToString());
                var user = await _userManager.GetUserAsync(HttpContext.User);
                model.User = user;
                model.posterUrl = user.ImagePath;
                _context.Jobs.Add(model);

                await _context.SaveChangesAsync();

                return RedirectToActionPermanent("Index", "Home");
            }

            return View("Create", model);
        }

        [HttpPost]
        //[Authorize(Roles = "Employee")]
        public async Task<IActionResult> Apply(int id, IFormFile upload,
            [Bind("User", "Job", "CVPath", "CreatedAt")]
             JobApplicantsViewModel model ,[FromServices] IFluentEmail mailer)
        {

            var job = _context.Jobs.SingleOrDefault(x => x.Id == id);
            var user = await _userManager.GetUserAsync(HttpContext.User);

            if (upload != null && upload.Length > 0)
            {
                var fileName = Path.GetFileName(upload.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);

                var updatedFilePath = filePath.Substring(filePath.IndexOf("/"));

                model.CVPath = updatedFilePath;
                //_context.Users.Add(model);
                //_context.SaveChanges();

                using (var fileSrteam = new FileStream(filePath, FileMode.Create))
                {
                    await upload.CopyToAsync(fileSrteam);
                }
            }

            if (user == null)
            {
                return RedirectToActionPermanent("Login", "Account");
            }
            else
            {
                if (!User.IsInRole("Employee"))
                {
                    TempData["message"] = "You can't do this action";
                    return RedirectToActionPermanent("JobDetails", "Home", new { id });
                }
            }
            var apply = new Applicant
            {
                User = user,
                Job = job,
                CVPath = model.CVPath,
                CreatedAt = DateTime.Now
            };

            //EMAIL FUNCTION
            var sender = new SmtpSender(() => new SmtpClient("smtp.gmail.com")
            {
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Port = 587,
                Credentials = new NetworkCredential("mymicrowaveisdry@gmail.com", "")
                //DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory,
                //PickupDirectoryLocation = @"C:\emails"
            });

            Email.DefaultSender = sender;
            Email.DefaultRenderer = new RazorRenderer();

            StringBuilder template = new StringBuilder();
            template.AppendLine("Dear @Model.FirstName,");
            template.AppendLine("<p>Thank you for your @Model.Job application. You'll be notified once we finish reviewing CV.</p>");
            template.AppendLine("- ABC Bebitos");

            var email = await Email
                .From("mymicrowaveisdry@gmail.com")
                .To("eglitafa008@gmail.com", "Egli")
                .Subject("Application Sent Succesfully!")
                .UsingTemplate(template.ToString(), new { FirstName = apply.User.FirstName, Job = job.Title })
                .SendAsync();

            _context.Applicants.Add(apply);

            await _context.SaveChangesAsync();

            return RedirectToActionPermanent("JobDetails", "Home", new { id });
        }

        [Route("mark-as-filled/{id}")]
        [Authorize(Roles = "Employer")]
        public async Task<IActionResult> MarkAsFilled(int id)
        {
            var job = _context.Jobs.SingleOrDefault(x => x.Id == id);
            job.Filled = true;
            _context.Jobs.Update(job);
            await _context.SaveChangesAsync();

            return RedirectToActionPermanent("Index", "Dashboard");
        }
        
        [Authorize(Roles = "Employer")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var job = await _context.Jobs
                .FirstOrDefaultAsync(m => m.Id == id);
            if (job == null)
            {
                return NotFound();
            }

            return View(job);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var job = await _context.Jobs.FindAsync(id);
            _context.Jobs.Remove(job);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var job = await _context.Jobs.FindAsync(id);
            if (job == null)
            {
                return NotFound();
            }
            return View(job);
        }

        // POST: HomePage/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Employer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id", "Title", "Description", "Category", "Location", "Type", "CompanyName", "CompanyDescription", "Website", "Salary", "LastDate")] Job job)
        {
            if (id != job.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(job);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if ((job.Id) != job.Id)
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(job);
        }

    }
}