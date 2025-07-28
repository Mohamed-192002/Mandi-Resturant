using Core.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class Customer:EntityBaseName
    {
        public string Phone { get; set; } = string.Empty;
        public string? AnotherPhone { get; set; }
        public string Address { get; set; } = string.Empty;
        public string? Address2 { get; set; } 
        public string? Address3 { get; set; } 
        public string? Address4 { get; set; }

    }
}
