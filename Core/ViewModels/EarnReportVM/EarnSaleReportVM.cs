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
        public int Nafr { get; set; }
        public int HalfNafr { get; set; }
        public int Dagag { get; set; }
        public int HalfDagag { get; set; }
        public int TotalNafrAmount => Nafr * TotalAmount;
        public int TotalHalfNafrAmount => HalfNafr * TotalAmount;
        public double TotalDagagAmount => (Dagag * TotalAmount) + (HalfDagag * TotalAmount * 0.5);
        public string NafrDisplay => TotalNafrAmount > 0 ? TotalNafrAmount.ToString() : "-";
        public string HalfNafrDisplay => TotalHalfNafrAmount > 0 ? TotalHalfNafrAmount.ToString() : "-";
        public string DagagDisplay => TotalDagagAmount > 0 ? TotalDagagAmount.ToString() : "-";
    }
}
