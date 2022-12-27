using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace JobPortal.ViewModels
{
    public class EmployerViewModel
    {
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        public List<IdentityUserRole<string>> Roles { get; set; }
    }
}
