using JobPortal.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JobPortal.ViewModels.Home
{
    public class JobDetailsViewModel
    {
        public IList<Job> JobList { get; set; }
        public Job Job { get; set; }
        public IFormFile MyPdf { set; get; }
        public bool IsApplied { get; set; }
        public string PhoneNumber { get; set; }
        public string CompanyPhoneNumber { get; set; }
        public string PosterUrl { get; set; }
        public string PosterImageUrl { get; set; }
        public int ViewCounter { get; set; }
    }
}
