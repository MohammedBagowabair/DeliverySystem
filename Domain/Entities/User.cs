using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class User : Person
    {
        public string Email { get; set; }             // Email used as the username for login
        public string Password { get; set; }          // Password for authentication
        public string Role { get; set; }              // Role to differentiate roles (e.g., Admin, Customer)
    }

}
