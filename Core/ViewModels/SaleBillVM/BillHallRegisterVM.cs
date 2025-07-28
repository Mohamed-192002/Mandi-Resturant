using Core.Common.Enums;

namespace Core.ViewModels.SaleBillVM
{
    public class BillHallRegisterVM
    {
        public List<BillDetailRegisterVM>? BillDetailRegisterVM { get; set; }
        public List<string>? TableIds { get; set; }
        public BillType BillType { get; set; }
        public DateTime Date { get; set; }
        public double Total { get; set; }
        public double Discount { get; set; }
        public double Vat { get; set; }
        public double FinalTotal { get; set; }
        public string? Notes { get; set; }
    }
}
