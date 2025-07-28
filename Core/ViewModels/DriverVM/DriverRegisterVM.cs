using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.ViewModels.DriverVM
{
    public class DriverRegisterVM
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "هذا الحقل مطلوب")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "هذا الحقل مطلوب")]
        public string Phone { get; set; } = string.Empty;
        public Guid CreatedUser { get; set; }
        public Guid LastEditUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastEditDate { get; set; }
    }
}
