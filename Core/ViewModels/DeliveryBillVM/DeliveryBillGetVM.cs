using Core.Common.Enums;
using Core.ViewModels.SaleBillVM;

namespace Core.ViewModels.DeliveryBillVM
{
    public class DeliveryBillGetVM
    {
        public int Id { get; set; }
        public int DeliveryId { get; set; }
        public string? DeliveryName { get; set; }
        public int DriverId { get; set; }
        public string? DriverName { get; set; }
        public string? OrderNumber { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerAddress { get; set; }
        public string? CustomerPhone { get; set; }
        public DateTime Date { get; set; }
        public double DeliveryPrice { get; set; }
        public double FinalTotal { get; set; }
        public bool MoneyDelivered { get; set; }
        public List<BillDetailRegisterVM>? BillDetailRegisterVM { get; set; }
        public TimeOnly? OrderDeliveredTime { get; set; }
        public BillType BillType { get; set; }
    }
}
