using JobPortal.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace JobPortal.ViewModels
{
    public class EmployeeRegisterViewModel
    {
        public User User { get; set; }

        [MaxLength(60), Display(Name = "User Name", Prompt = "User Name")]
        public string UserName { get; set; }

        [Required, MaxLength(20), Display(Name = "First Name", Prompt = "First Name")]
        public string FirstName { get; set; }

        [Required, MaxLength(60), Display(Name = "Last Name", Prompt = "Last Name")]
        public string LastName { get; set; }

        [Required, MaxLength(60), Display(Name = "Email", Prompt = "Email")]
        [EmailAddress]
        public string Email { get; set; }

        [MaxLength(200), Display(Name = "Description", Prompt = "Description")]
        public string Description { get; set; }

        [Required, MaxLength(60), Display(Name = "Phone Number", Prompt = "Phone Number")]
        [Phone]
        public string PhoneNumber { get; set; }

        [Display(Name ="Profile Picture")]
        public IFormFile? Photo { get; set; }

        public string ImagePath { get; set; }

        [Required, DataType(DataType.Password), Display(Name = "Password", Prompt = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password), Compare(nameof(Password)), Display(Name = "Confirm Password", Prompt = "Confirm Password")]
        public string ConfirmPassword { get; set; }
    }
}
