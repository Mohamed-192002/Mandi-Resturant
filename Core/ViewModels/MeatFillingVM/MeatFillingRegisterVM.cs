using Core.Common;
using System.ComponentModel.DataAnnotations;

namespace Core.ViewModels.MeatFillingVM
{
    public class MeatFillingRegisterVM
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "هذا الحقل مطلوب")]
        public int HoleId { get; set; }
        public List<CommonDrop>? Holes { get; set; }

        [Required(ErrorMessage = "هذا الحقل مطلوب")]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "هذا الحقل مطلوب")]
        public int Nafr { get; set; }

        [Required(ErrorMessage = "هذا الحقل مطلوب")]
        public int HalfNafr { get; set; }

        [Required(ErrorMessage = "هذا الحقل مطلوب")]
        public TimeOnly EndTime { get; set; }
        public Guid CreatedUser { get; set; }
        public Guid LastEditUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastEditDate { get; set; }
    }
}
