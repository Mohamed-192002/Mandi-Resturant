using Core.Common;
using Core.ViewModels.DeliveryBillVM;

namespace Core.ViewModels.ResturantDeliveryVM
{
    public class ResturantDeliveryMainVM
    {
        public List<DeliveryBillGetVM>? DeliveryBillGetVM { get; set; }
        public List<CommonDrop>? Drivers { get; set; }
    }
}
