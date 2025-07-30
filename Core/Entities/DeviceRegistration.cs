using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities
{
    public class DeviceRegistration
    {
        public int Id { get; set; }
        public string? DeviceId { get; set; }  // من Flutter
        public string AccessCode { get; set; } // الكود اللى دخل الكاشير
        public Guid UserId { get; set; }       // مربوط بالكاشير
        [ForeignKey(nameof(UserId))]
        public User User { get; set; } // الكاشير اللى مسجل الجهاز
        public bool IsDeleted { get; set; }
    }

}
