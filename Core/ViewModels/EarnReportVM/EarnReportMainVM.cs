using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.ViewModels.EarnReportVM
{
    public class EarnReportMainVM
    {
        [Required(ErrorMessage = "هذا الحقل مطلوب")]
        public DateTime? FromDate { get; set; }

        [Required(ErrorMessage = "هذا الحقل مطلوب")]
        public DateTime? ToDate { get; set; }
        public List<EarnSaleReportVM>? EarnSaleReportVM { get; set; }
        public double TotalSaleEarn { get; set; }
        public double TotalExpenses { get; set; }
        public double FinalEarn { get; set; }
    }
}
