using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace JobPortal.Models
{
    public class Applicant
    {
        public int Id { get; set; }
        public User User { get; set; }
        public Job Job { get; set; }

        public string CVPath { get; set; }

        public string ProfilePicture { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
