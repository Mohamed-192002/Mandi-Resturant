using Core.Common;

namespace Core.ViewModels.DeliveryBillVM
{
    public class DeliveryBillReportVM
    {
        public int? DeliveryId { get; set; }
        public List<CommonDrop>? Deliveries { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public List<DeliveryBillGetVM> DeliveryBillGetVM { get; set; } = new List<DeliveryBillGetVM>();
        public double TotalPaid { get; set; }
        public double TotalUnPaid { get; set; }
        public int TotalTotalNafr { get; set; }
        public int TotalTotalHalfNafr { get; set; }
        public int TotalTotalDagag { get; set; }
    }
}
