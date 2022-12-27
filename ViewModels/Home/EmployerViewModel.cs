using JobPortal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JobPortal.ViewModels.Home
{
    public class EmployerViewModel
    {
        public IList<Job> Job { get; set; }
        public IList<User> Employers { get; set; }
    }
}
