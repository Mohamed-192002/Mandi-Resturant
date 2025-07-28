using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Core.ViewModels.DeliveryVM
{
    public class DeliveryRegisterVM
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "هذا الحقل مطلوب")]
        public string Name { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public IFormFile? ImageFile { get; set; }
        public string? Color { get; set; }
        public Guid CreatedUser { get; set; }
        public Guid LastEditUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastEditDate { get; set; }
    }
}
