namespace Core.ViewModels.SaleBillVM
{
    public class CheckDeliveryHoleAmountVM
    {
        public List<BillDetailRegisterVM>? BillDetailRegisterVM { get; set; }
        public List<string>? HoleIds { get; set; }
        public string? OrderDeliveredTime { get; set; }
    }
}
