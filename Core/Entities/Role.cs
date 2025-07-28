using Microsoft.AspNetCore.Identity;

namespace Core.Entities
{
    public class Role : IdentityRole<Guid>
    {

        public ICollection<UserRole>? UserRole { get; set; }
        public ICollection<RoleClaims>? RoleClaims { get; set; }
    }
}
