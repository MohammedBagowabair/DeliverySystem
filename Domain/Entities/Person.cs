using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public abstract class Person : BaseEntity
    {
        public string FullName { get; set; }          // Person's full name
        public string PhoneNumber1 { get; set; }       // Primary phone number
        public string PhoneNumber2 { get; set; }       // Secondary phone number (optional)
        public string Address { get; set; }            // Address of the person
    }

}
