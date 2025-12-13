using Core.ViewModels.SaleBillVM;

namespace Core.ViewModels.SaleBillPrintVM
{
    public class BillHallPrintVM
    {
        public string? Billnumber { get; set; }
        public string? TableName { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerAddress { get; set; }
        public string? CustomerPhone { get; set; }
        public string? DeliveryName { get; set; }
        public string? DriverName { get; set; }
        public DateTime Date { get; set; }
        public List<BillDetailRegisterVM> BillDetailRegisterVM { get; set; } = new List<BillDetailRegisterVM>();
        public double Discount { get; set; }
        public double Vat { get; set; }
        public double DeliveryPrice { get; set; }
        public double TotalPrice { get; set; }
        public string? Notes { get; set; }
        public TimeOnly? OrderDeliveredTime { get; set; }
        public string? CashierName { get; set; }
        public string? OrderNumber { get; set; }

    }
}
