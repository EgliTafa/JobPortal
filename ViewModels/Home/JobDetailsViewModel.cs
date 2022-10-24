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
        public Job Job { get; set; }
        public IFormFile MyPdf { set; get; }
        public bool IsApplied { get; set; }

        public string PosterUrl { get; set; }
    }
}
