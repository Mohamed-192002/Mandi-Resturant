
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.ViewModels.SaleBillVM
{
    public class CompleteTempMainVM
    {
        public int? SaleBillId { get; set; }
        public double Discount { get; set; }
        public double Vat { get; set; }
        public double FinalTotal { get; set; }
        public string? Notes { get; set; }
        public List<BillDetailRegisterVM> BillDetailRegisterVM { get; set; } = new List<BillDetailRegisterVM>();
    }
}
