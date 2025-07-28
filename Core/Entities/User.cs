using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class User : IdentityUser<Guid>
    {

        public string? Name { get; set; }
        public bool IsDeleted { get; set; }
        public string? RealPassword { get; set; }
        public virtual ICollection<UserRole>? UserRole { get; set; }

    }
}
