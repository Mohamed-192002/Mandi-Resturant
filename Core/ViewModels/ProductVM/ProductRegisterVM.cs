using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Core.ViewModels.ProductVM
{
    public class ProductRegisterVM
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "هذا الحقل مطلوب")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "هذا الحقل مطلوب")]
        public double? CostPrice { get; set; }

        [Required(ErrorMessage = "هذا الحقل مطلوب")]
        public double? SellingPrice { get; set; }
        public int? Nafr { get; set; }
        public int? HalfNafr { get; set; }
        public int? Dagag { get; set; }
        public int? HalfDagag { get; set; }
        public string? ImageUrl { get; set; }
        public IFormFile? ImageFile { get; set; }
        public Guid CreatedUser { get; set; }
        public Guid LastEditUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastEditDate { get; set; }
    }
}
