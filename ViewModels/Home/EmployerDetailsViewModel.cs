using JobPortal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JobPortal.ViewModels.Home
{
    public class EmployerDetailsViewModel
    {
        public IList<Job> Job { get; set; }
        public IList<User> Employers { get; set; }
        public User CurrentEmployer { get; set; }
        public string CompanyPhoneNumber { get; set; }
        public string CompanyDescription { get; set; }
        public string CompanyName { get; set; }
        public string PosterImageUrl { get; set; }
        public int JobCounter { get; set; }
    }
}
