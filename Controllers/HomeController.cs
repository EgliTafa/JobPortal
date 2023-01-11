using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using JobPortal.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using JobPortal.ViewModels;
using JobPortal.ViewModels.Home;

namespace JobPortal.Controllers
{
    public class HomeController : Controller
    {
        public IList<Job> JobList
        {
            get;
            set;
        }
        public IList<User> EmployersList
        {
            get;
            set;
        }
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public HomeController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int p = 1, int s = 5, int TotalRecords = 0, int count = 0)
        {
            TotalRecords = _context.Jobs.Count();

            var trendings = _context.Jobs
              .Where(b => b.CreatedAt.Month == DateTime.Now.Month)
              .Where(x => x.Filled == false)
              .ToList();

            JobList = await _context.Jobs
              //.OrderByDescending(x => x.ViewCount)
              .OrderByDescending(x => x.CreatedAt)
              //.Skip((p - 1) * s)
              //.Take(s)
              .ToListAsync();

            var model = new TrendingJobViewModel
            {
                Trendings = trendings,
                TotalRecords = TotalRecords,
                P = p,
                S = s,
                JobList = JobList
            };

            return View(model);
        }

        [Route("jobs/{id}/details")]
        public async Task<IActionResult> JobDetails(int id, int count = 0)
        {
            //ViewBag.message = "You can't do this action";
            var job = _context.Jobs.FirstOrDefault(x => x.Id == id);

            JobList = await _context.Jobs
              //.OrderByDescending(x => x.ViewCount)
              .OrderByDescending(x => x.CreatedAt)
              .Take(10)
              .ToListAsync();

            count++;
            job.ViewCount = job.ViewCount + count;
            _context.Update(job);
            await _context.SaveChangesAsync();
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var applied = false;
            if (user != null)
                applied = _context.Applicants.Where(x => x.Job == job).Any(x => x.User == user);

            var model = new JobDetailsViewModel
            {
                Job = job,
                JobList = JobList,
                IsApplied = applied
            };
            return View(model);
        }

        [Route("search")]
        public async Task<IActionResult> Search(string searchString, string searchCategory, string searchShifts, int? id)
        {
            //creates a LINQ Query to select jobs
            var job = from j in _context.Jobs
                      select j;

            //checks if empty and get matching data

            if (!String.IsNullOrEmpty(searchString) && !String.IsNullOrEmpty(searchCategory) && !String.IsNullOrEmpty(searchShifts))
            {
                job = job.Where(s => s.Title!.Contains(searchString) && s.Category!.Contains(searchCategory) && s.Type!.Contains(searchShifts));
            }
            else if (!String.IsNullOrEmpty(searchString) && !String.IsNullOrEmpty(searchCategory))
            {
                job = job.Where(s => s.Title!.Contains(searchString) && s.Category!.Contains(searchCategory));
            }
            else if (!String.IsNullOrEmpty(searchString) && !String.IsNullOrEmpty(searchShifts))
            {
                job = job.Where(s => s.Title!.Contains(searchString) && s.Type!.Contains(searchShifts));
            }
            else if (!String.IsNullOrEmpty(searchCategory) && !String.IsNullOrEmpty(searchShifts))
            {
                job = job.Where(s => s.Category!.Contains(searchCategory) && s.Type!.Contains(searchShifts));
            }
            else if (!String.IsNullOrEmpty(searchString))
            {
                job = job.Where(s => s.Title!.Contains(searchString));
            }
            else if (!String.IsNullOrEmpty(searchCategory))
            {
                job = job.Where(s => s.Category!.Contains(searchCategory));
            }
            else if (!String.IsNullOrEmpty(searchShifts))
            {
                job = job.Where(s => s.Type!.Contains(searchShifts));
            }

            return View(await job.ToListAsync());
        }

        [Route("privacy")]
        public IActionResult Privacy()
        {
            return View();
        }

        [Route("employer/all-employers")]
        public async Task<IActionResult> AllEmployers(string id)
        {
            var employer = await _userManager.GetUsersInRoleAsync("Employer");
            EmployersList = employer.ToList();

            var jobByEmployer = _context.Jobs.OrderBy(x => x.User.Id).ToList();

            var model = new EmployerViewModel
            {
                Employers = EmployersList,
                Job = jobByEmployer
            };

            return View(model);
        }

        [Route("employer/{id}/details")]
        public async Task<IActionResult> EmployerDetails(int jobId, string id, int count = 0)
        {
            var employer = await _userManager.GetUsersInRoleAsync("Employer");
            EmployersList = employer.ToList();
            var currentEmployers = EmployersList.FirstOrDefault(x => x.Id == id);

            var jobByEmployer = _context.Jobs.Where(g => g.User.Id == id).ToList();

            var model = new EmployerDetailsViewModel
            {
                Job = jobByEmployer,
                Employers = EmployersList,
                CurrentEmployer = currentEmployers,
                //CompanyPhoneNumber = job.User.PhoneNumber,
                //CompanyDescription = job.User.Description,
                //CompanyName = job.User.FirstName
            };

            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}