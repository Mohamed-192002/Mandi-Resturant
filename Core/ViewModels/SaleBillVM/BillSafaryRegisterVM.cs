using Core.Common.Enums;

namespace Core.ViewModels.SaleBillVM
{
    public class BillSafaryRegisterVM
    {
        public int? Id { get; set; }
        public List<BillDetailRegisterVM>? BillDetailRegisterVM { get; set; }
        public BillType BillType { get; set; }
        public int? CustomerId { get; set; }
        public DateTime Date { get; set; }
        public double Total { get; set; }
        public double Discount { get; set; }
        public double Vat { get; set; }
        public double FinalTotal { get; set; }
        public string? Notes { get; set; }
        public bool Gift { get; set; }
        public string? CustomerAddress { get; set; }
    }
}
