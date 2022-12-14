using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace JobPortal.ViewModels
{
    public class EmployerRegisterViewModel
    {
        [MaxLength(60)]
        public string UserName { get; set; }

        [Required, MaxLength(20)]
        public string FirstName { get; set; }

        [Required, MaxLength(60)]
        public string LastName { get; set; }

        [Required, MaxLength(60)]
        [EmailAddress]
        public string Email { get; set; }

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }

        [Required, MaxLength(60), Display(Name = "Phone Number", Prompt = "Phone Number")]
        [Phone]
        public string PhoneNumber { get; set; }

        [Display(Name = "Profile Picture")]
        public IFormFile? Photo { get; set; }

        public string ImagePath { get; set; }

        [DataType(DataType.Password), Compare(nameof(Password))]
        public string ConfirmPassword { get; set; }
    }
}
