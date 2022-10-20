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
        public int P { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int S { get; set; } = 5;
        public List<Job> Jobs { get; set; }
        public List<Job> Trendings { get; set; }

        public int TotalRecords { get; set; } = 0;
    }
}
