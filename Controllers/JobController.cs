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
        public async Task<IActionResult> Save(Job model)
        {
            if(ModelState.IsValid)
            {
                TempData["type"] = "success";
                TempData["message"] = "Job posted successfully";
                //_logger.LogInformation(model.ToString());
                var user = await _userManager.GetUserAsync(HttpContext.User);
                model.User = user;
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
             JobApplicantsViewModel model)
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
                if(!User.IsInRole("Employee"))
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
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Category,Location,Type,CompanyName,CompanyDescription,Website,Salary")] Job job)
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


//public async Task<IActionResult> Destroy(int id)
//{
//    var job = _context.Jobs.SingleOrDefault(x => x.Id == id);
//    if(job == null)
//    {
//        return NotFound();
//    }

//    _context.Jobs.Remove(job);
//    await _context.SaveChangesAsync();

//    TempData["type"] = "success";
//    TempData["message"] = "Job deleted successfully";

//    return RedirectToActionPermanent("Index", "Dashboard");
//}