using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JobPortal.Models
{
    public class Employers
    {
        public int Id { get; set; }
        public User User { get; set; }
        public Job Job { get; set; }
        public string CompanyDescription { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
