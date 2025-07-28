using System.ComponentModel.DataAnnotations;

namespace Core.Dtos.AuthDto
{
    public class LoginDto
    {

        [Required(ErrorMessage = "هذا الحقل مطلوب")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "هذا الحقل مطلوب")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";
    }
}
