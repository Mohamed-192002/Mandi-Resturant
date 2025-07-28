namespace Core.ViewModels.SaleBillVM
{
    public class BillDeliveryRegisterVM
    {
        public List<BillDetailRegisterVM>? BillDetailRegisterVM { get; set; }
        public List<string>? HoleIds { get; set; }
        public int DeliveryId { get; set; }
        public int? CustomerId { get; set; }
        public double Discount { get; set; }
        public double Vat { get; set; }
        public double FinalTotal { get; set; }
        public string? OrderDeliveredTime { get; set; }
        public string? Notes { get; set; }
        public bool CustomerReceive { get; set; }
        public string? CustomerAddress { get; set; }
    }
}
