using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace LeaveTracker
{
    public class User
    {
        [Display(Order = 0)]
        public string Username { get; set; }

        [Display(Order = 1)]
        public string Password { get; set; }

        [Display(Order = 2)]
        public string Name { get; set; }

        [Display(Order = 3)]
        public int Access { get; set; }

        [Display(Order = 4)]
        public int LeaveCount { get; set; }

        [Display(Order = 5)]
        public int TotalLeave { get; set; }
    }

    
}
