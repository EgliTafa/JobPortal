using JobPortal.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JobPortal.ViewModels.Home
{
    public class TrendingJobViewModel
    {
        [BindProperty(SupportsGet = true)]
        public int P { get; set; }

        [BindProperty(SupportsGet = true)]
        public int S { get; set; } 
        public IList<Job> JobList { get; set; }
        public List<Job> Trendings { get; set; }
        public List<User> Employer { get; set; }
        public int TotalRecords { get; set; } 
    }
}
