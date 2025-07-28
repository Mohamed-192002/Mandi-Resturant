namespace Core.ViewModels.SaleBillVM
{
    public class BillDetailRegisterVM
    {
        public int SaleBillId { get; set; }
        public int ProductId { get; set; }
        public string? PName { get; set; }
        public double Price { get; set; }
        public int Amount { get; set; }
        public double Discount { get; set; }
        public double TotalPrice { get; set; }
    }
}
