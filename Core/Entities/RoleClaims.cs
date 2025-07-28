using Microsoft.AspNetCore.Identity;

namespace Core.Entities
{
    public class RoleClaims : IdentityRoleClaim<Guid>
    {
        public Role? Role { get; set; }
    }
}
