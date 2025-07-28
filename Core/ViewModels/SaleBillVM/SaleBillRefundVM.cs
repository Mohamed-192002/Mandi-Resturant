using Core.Common.Enums;

namespace Core.ViewModels.SaleBillVM
{
    public class SaleBillRefundVM
    {
        public int Id { get; set; }
        public BillType BillType { get; set; }
        public List<BillDetailRegisterVM>? BillDetailRegisterVM { get; set; }
    }
}
