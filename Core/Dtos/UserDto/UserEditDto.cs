using Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Dtos.UserDto
{
  public  class UserEditDto
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "هذا الحقل مطلوب")]
        [Display(Name = "Name")]
        public string Name { get; set; } = "";

        [Required(ErrorMessage = "هذا الحقل مطلوب")]
        [Display(Name = "Phone")]
        [StringLength(10, ErrorMessage = "أدخل رقم هاتف صحيح", MinimumLength = 10)]
        public string PhoneNumber { get; set; } = "";

        [Required(ErrorMessage = "هذا الحقل مطلوب")]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = "";

        public List<CommonDto>? Roles { get; set; }
        

    }
}
