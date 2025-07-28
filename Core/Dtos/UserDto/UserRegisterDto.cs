using System.ComponentModel.DataAnnotations;

namespace Core.Dtos.UserDto
{
    public class UserRegisterDto
    {
        [Required(ErrorMessage = "هذا الحقل مطلوب")]
        [Display(Name = "Name")]
        public string Name { get; set; } = "";

        [Required(ErrorMessage = "هذا الحقل مطلوب")]
        [StringLength(10, ErrorMessage = "أدخل رقم هاتف صحيح", MinimumLength = 10)]
        [Display(Name = "Phone")]
        public string PhoneNumber { get; set; } = "";

        [Required(ErrorMessage = "هذا الحقل مطلوب")]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "هذا الحقل مطلوب")]
        [StringLength(100, ErrorMessage = "يجب أن يكون الباسوورد أكبر من 6 وأقل من 100", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = "";

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "هذا الحقل مطلوب")]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "تأكيد الباسوورد غير مطابق مع الباسوورد")]
        public string ConfirmPassword { get; set; } = "";

        [Required(ErrorMessage = "هذا الحقل مطلوب")]
        public List<CommonDto> Roles { get; set; } = new List<CommonDto>();


    }
}
