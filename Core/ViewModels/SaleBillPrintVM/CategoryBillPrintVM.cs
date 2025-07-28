using Core.ViewModels.SaleBillVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.ViewModels.SaleBillPrintVM
{
    public class CategoryBillPrintVM
    {
        public string? CategoryName { get; set; }
        public string? Billnumber { get; set; }
        public DateTime Date { get; set; }
        public string? Notes { get; set; }
        public List<BillDetailRegisterVM> BillDetailRegisterVM { get; set; } = new List<BillDetailRegisterVM>();
    }
}
