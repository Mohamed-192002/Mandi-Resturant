using System.ComponentModel.DataAnnotations;

namespace Core.ViewModels.CustomerVM
{
    public class CustomerRegisterVM
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "*")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "*")]
        public string Phone { get; set; } = string.Empty;
        public string? AnotherPhone { get; set; }

        [Required(ErrorMessage = "*")]
        public string Address { get; set; } = string.Empty;
        public string? Address2 { get; set; }
        public string? Address3 { get; set; }
        public string? Address4 { get; set; }
        
        // Delivery fields
        public double? DeliveryPrice { get; set; }
        public string? DeliveryRegion { get; set; }
        
        // Delivery time field
        public TimeOnly? OrderDeliveredTime { get; set; }
        
        public Guid CreatedUser { get; set; }
        public Guid LastEditUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastEditDate { get; set; }
    }
}
