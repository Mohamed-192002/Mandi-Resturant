using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities
{
    public class PrinterRegistration
    {
        public int Id { get; set; }
        public string IpAddress { get; set; }
        public string Name { get; set; }
        public Guid UserId { get; set; }       // مربوط بالكاشير
        [ForeignKey(nameof(UserId))]
        public User User { get; set; } // الكاشير اللى مسجل الجهاز
        public bool IsDeleted { get; set; }
    }
}
