using Core.Entities;
using Core.ViewModels.EarnReportVM;
using Core.Common.Enums;

namespace Core.ViewModels.CashierSaleReportVM
{
    public class CashierSaleMainVM
    {
        public Guid? UserId { get; set; }
        public List<User>? Users { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public BillType? BillType { get; set; }
        public List<EarnSaleReportVM>? EarnSaleReportVM { get; set; }
        public double TotalSale { get; set; }
        public double TotalEarn { get; set; }
    }
}
