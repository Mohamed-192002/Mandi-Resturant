using Microsoft.AspNetCore.Identity;

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
