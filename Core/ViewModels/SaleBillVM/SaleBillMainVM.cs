using Core.Common;
using Core.ViewModels.CustomerVM;
using Core.ViewModels.DeliveryVM;
using Core.ViewModels.HoleVM;
using Core.ViewModels.ProductVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.ViewModels.SaleBillVM
{
    public class SaleBillMainVM
    {
        public List<DeliveryGetVM>? Deliveries { get; set; }
        public CustomerRegisterVM CustomerRegisterVM { get; set; } = new CustomerRegisterVM();
        public List<SaleBillRefundVM>? SaleBillRefundVM { get; set; }
        public List<CommonDrop>? SaleBills { get; set; }
        public List<CommonDrop>? Products { get; set; }
        public List<ProductGetVM>? ProductGetVM { get; set; }
        public List<HoleGetVM>? HoleGetVM { get; set; }
        public List<DagagHoleDataVM>? DagagHoleDataVM { get; set; }
        public List<MeatHoleDataVM>? MeatHoleDataVM { get; set; }

    }
}
