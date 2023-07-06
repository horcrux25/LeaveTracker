using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaveTracker
{
    public class User
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public int Access { get; set; }
        public int LeaveCount { get; set; }
        public int TotalLeave { get; set; }
    }
}
