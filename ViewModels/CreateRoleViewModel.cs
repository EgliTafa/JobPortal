using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace JobPortal.ViewModels
{
    public class CreateRoleViewModel : Controller
    {
        [Required]
        [Display(Name = "Role")]
        public string RoleName { get; set; } = "";
    }
}
