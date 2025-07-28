namespace Core.ViewModels.EarnReportVM
{
    public class EarnSaleReportVM
    {
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public double CostPrice { get; set; }
        public double SalePrice { get; set; }
        public double ProductEarn { get; set; }
        public int TotalAmount { get; set; }
        public double TotalSalePrice { get; set; }
        public double TotalProductEarn { get; set; }
    }
}
