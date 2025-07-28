using System.ComponentModel.DataAnnotations;

namespace Core.Dtos.UserDto
{
    public class ChangeUserPasswordDto
    {
        public Guid? Id { get; set; }

        [Required(ErrorMessage = "هذا الحقل مطلوب")]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; } = "";

        [Required(ErrorMessage = "هذا الحقل مطلوب")]
        [StringLength(100, ErrorMessage = "يجب أن يكون الباسوورد أكبر من 6 وأقل من 100", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; } = "";

        [Required(ErrorMessage = "هذا الحقل مطلوب")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        [Compare("NewPassword", ErrorMessage = "تأكيد الباسوورد غير مطابق مع الباسوورد")]
        public string ConfirmNewPassword { get; set; } = "";
    }
}
