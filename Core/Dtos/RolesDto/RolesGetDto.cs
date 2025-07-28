using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Dtos.RolesDto
{
    public class RolesGetDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
    }
}
