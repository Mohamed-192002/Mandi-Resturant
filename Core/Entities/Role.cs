using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class Role : IdentityRole<Guid>
    {

        public ICollection<UserRole>? UserRole { get; set; }
        public ICollection<RoleClaims>? RoleClaims { get; set; }
    }
}
