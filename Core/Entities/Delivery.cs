using Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class Delivery:EntityBaseName
    {
        public string? ImageUrl { get; set; }
        public string? Color { get; set; }
    }
}
