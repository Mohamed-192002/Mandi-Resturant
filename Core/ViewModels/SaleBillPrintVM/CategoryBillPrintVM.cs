using Core.ViewModels.SaleBillVM;

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
