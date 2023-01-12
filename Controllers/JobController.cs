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
        public async Task<IActionResult> Save([Bind("User", "Title", "Description", "Website", "Location", "Type", "CompanyName", "CompanyDescription", "Job", "CVPath", "CreatedAt", "Salary", "Category", "LastDate", "posterUrl", "PosterImageUrl", "PreferredAge", "Education", "CompanyPhoneNumber")] Job model, IFormFile upload)
        {

            if (upload != null && upload.Length > 0)
            {
                var fileName = Path.GetFileName(upload.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img", fileName);

                var updatedFilePath = filePath.Substring(filePath.IndexOf("/img"));

                model.PosterImageURl = updatedFilePath;

                using (var fileSrteam = new FileStream(filePath, FileMode.Create))
                {
                    await upload.CopyToAsync(fileSrteam);
                }
            }

            if (ModelState.IsValid)
            {
                TempData["type"] = "success";
                TempData["message"] = "Job posted successfully";
                var user = await _userManager.GetUserAsync(HttpContext.User);

                model.User = user;
                int currentJobCount = user.JobCount;
                model.User.JobCount = currentJobCount + 1;

                model.CompanyDescription = user.Description;
                model.posterUrl = user.ImagePath;
                model.CompanyPhoneNumber = user.PhoneNumber;
                _context.Jobs.Add(model);

                await _context.SaveChangesAsync();

                return RedirectToActionPermanent("Index", "Home");
            }

            return View("Create", model);
        }

        [HttpPost]
        public async Task<IActionResult> Apply(int id, IFormFile upload,
          [Bind("User", "Job", "CVPath", "CreatedAt")] JobApplicantsViewModel model)
        {

            var job = _context.Jobs.SingleOrDefault(x => x.Id == id);
            var user = await _userManager.GetUserAsync(HttpContext.User);

            if (upload != null && upload.Length > 0)
            {
                var fileName = Path.GetFileName(upload.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);

                var updatedFilePath = filePath.Substring(filePath.IndexOf("/uploads"));

                model.CVPath = updatedFilePath;
                model.Gender = user.Gender;

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
                    return RedirectToActionPermanent("JobDetails", "Home", new
                    {
                        id
                    });
                }
            }
            var apply = new Applicant
            {
                User = user,
                Job = job,
                CVPath = model.CVPath,
                CreatedAt = DateTime.Now,
            };

            //EMAIL FUNCTION
            var sender = new SmtpSender(() => new SmtpClient("smtp.gmail.com")
            {
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Port = 587,
                Credentials = new NetworkCredential("#", "#")
                //DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory,
                //PickupDirectoryLocation = @"C:\emails"
            });

            Email.DefaultSender = sender;
            Email.DefaultRenderer = new RazorRenderer();

            StringBuilder template = new StringBuilder();
            template.AppendLine("Dear @Model.FirstName,");
            template.AppendLine("<p>Your application for @Model.Job job post has been succesessfully sent. You'll be notified once we finish reviewing CV.</p>");
            template.AppendLine("- ABC Bebitos");

            var email = await Email
              .From("noreply@zero.al")
              .To(user.Email)
              .Subject("Application Sent Succesfully!")
              .UsingTemplate(template.ToString(), new
              {
                  FirstName = apply.User.FirstName,
                  Job = job.Title
              })
              .SendAsync();

            _context.Applicants.Add(apply);

            await _context.SaveChangesAsync();

            return RedirectToActionPermanent("JobDetails", "Home", new
            {
                id
            });
        }

        [Route("mark-as-filled/{id}")]
        [Authorize(Roles = "Employer,Admin")]
        public async Task<IActionResult> MarkAsFilled(int id)
        {
            var job = _context.Jobs.SingleOrDefault(x => x.Id == id);
            job.Filled = true;
            _context.Jobs.Update(job);
            await _context.SaveChangesAsync();

            return RedirectToActionPermanent("Index", "Dashboard");
        }

        [Authorize(Roles = "Employer,Admin")]
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
        [Authorize(Roles = "Employer,Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, IFormFile upload, Job model, [Bind("Id", "Title", "Description", "Category", "Location", "Type", "CompanyName", "CompanyDescription", "Website", "Salary", "LastDate", "posterUrl", "PosterImageURl", "PreferredAge", "Education", "CompanyPhoneNumber")] Job job)
        {
            if (upload != null && upload.Length > 0)
            {
                var fileName = Path.GetFileName(upload.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);

                var updatedFilePath = filePath.Substring(filePath.IndexOf("/uploads"));
                model.PosterImageURl = updatedFilePath;
                //_context.Users.Add(model);
                //_context.SaveChanges();

                using (var fileSrteam = new FileStream(filePath, FileMode.Create))
                {
                    await upload.CopyToAsync(fileSrteam);
                }
            }

            if (id != job.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(HttpContext.User);
                try
                {
                    job.posterUrl = user.ImagePath;
                    job.CompanyPhoneNumber = user.PhoneNumber;
                    job.CompanyDescription = user.Description;
                    job.PosterImageURl = model.PosterImageURl;
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