using Core.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.ViewModels.ExpenseVM
{
    public class ExpenseRegisterVM
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "هذا الحقل مطلوب")]
        public int ExpenseTypeId { get; set; }
        public List<CommonDrop>? ExpenseTypes { get; set; }

        [Required(ErrorMessage = "هذا الحقل مطلوب")]
        public double? Payment { get; set; }

        [Required(ErrorMessage = "هذا الحقل مطلوب")]
        public DateTime Date { get; set; } = DateTime.Now;
        public string? Notes { get; set; }
        public Guid CreatedUser { get; set; }
        public Guid LastEditUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastEditDate { get; set; }
    }
}
