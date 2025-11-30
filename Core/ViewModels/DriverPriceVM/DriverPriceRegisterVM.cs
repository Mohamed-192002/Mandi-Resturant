using System.ComponentModel.DataAnnotations;

namespace Core.ViewModels.DriverPriceVM
{
    public class DriverPriceRegisterVM
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "هذا الحقل مطلوب")]
        public string Region { get; set; } = string.Empty;

        [Required(ErrorMessage = "هذا الحقل مطلوب")]
        [Range(0.01, double.MaxValue, ErrorMessage = "يجب أن يكون السعر أكبر من صفر")]
        public decimal DeliveryPrice { get; set; }

        public Guid CreatedUser { get; set; }
        public Guid LastEditUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastEditDate { get; set; }
    }
}
