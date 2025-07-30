using System.ComponentModel.DataAnnotations;

namespace Core.Entities
{
    public class PhoneNumberRecord
    {
        [Key]
        public int Id { get; set; }
        public string PhoneNumber { get; set; }
        public Guid UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
