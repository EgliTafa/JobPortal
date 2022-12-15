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

        [Required(ErrorMessage = "Emri nuk mundet të jetë bosh."), MaxLength(20)]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Adresa nuk mundet të jetë bosh."), MaxLength(60)]
        public string LastName { get; set; }

        [Required(ErrorMessage = "E-Mail nuk mundet të jetë bosh."), MaxLength(60)]
        [EmailAddress]
        public string Email { get; set; }

        [MaxLength(200), Display(Name = "Description", Prompt = "Description")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Fjalëkalimi nuk mundet të jetë bosh."), DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Numri I Telefonit nuk mundet të jetë bosh."), MaxLength(60), Display(Name = "Phone Number", Prompt = "Phone Number")]
        [Phone]
        public string PhoneNumber { get; set; }

        [Display(Name = "Profile Picture")]
        public IFormFile? Photo { get; set; }

        public string ImagePath { get; set; }

        [DataType(DataType.Password), Compare(nameof(Password), ErrorMessage =
            "Fjalëkalimet nuk janë njësoj. Jepni fjalëkalimin e duhur.")]
        public string ConfirmPassword { get; set; }
    }
}
