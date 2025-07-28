using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
