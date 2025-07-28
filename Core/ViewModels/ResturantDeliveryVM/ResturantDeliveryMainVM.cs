using Core.Common;
using Core.ViewModels.DeliveryBillVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.ViewModels.ResturantDeliveryVM
{
    public class ResturantDeliveryMainVM
    {
        public List<DeliveryBillGetVM>? DeliveryBillGetVM { get; set; }
        public List<CommonDrop>? Drivers { get; set; }
    }
}
