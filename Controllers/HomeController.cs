using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using JobPortal.Models;
using JobPortal.ViewModels.Home;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.Controllers
{
    public class HomeController : Controller
    {
        public IList<Job> JobList { get; set; }
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
                        .OrderBy(x => x.Id)
                        .Skip((p - 1) * s)
                        .Take(s)
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
        public async Task<IActionResult> JobDetails(int id)
        {
            //ViewBag.message = "You can't do this action";
            var job = _context.Jobs.FirstOrDefault(x => x.Id == id);
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var applied = false;
            if (user != null)
                applied = _context.Applicants.Where(x => x.Job == job).Any(x => x.User == user);

            var model = new JobDetailsViewModel
            {
                Job = job,
                IsApplied = applied
            };
            return View(model);
        }

        [Route("search")]
        public async Task<IActionResult> Search(string searchString, int? id)
        {
            //creates a LINQ Query to select jobs
            var job = from j in _context.Jobs
                      select j;

            //checks if empty and get matching data
            if (!String.IsNullOrEmpty(searchString))
            {
                job = job.Where(s => s.Title!.Contains(searchString));
            }

            return View(await job.ToListAsync());
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}