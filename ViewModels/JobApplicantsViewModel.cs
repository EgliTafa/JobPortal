using System.Collections.Generic;
using JobPortal.Models;

namespace JobPortal.ViewModels
{
    public class JobApplicantsViewModel
    {
        public Job Job { get; set; }

        public string CVPath { get; set; }
        public string Gender { get; set; }

        public List<Applicant> Applicants { get; set; }
    }
}