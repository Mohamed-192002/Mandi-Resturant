using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.ViewModels.DeliveryVM
{
    public class DeliveryMainVM
    {
        public DeliveryRegisterVM DeliveryRegisterVM { get; set; } = new DeliveryRegisterVM();
        public List<DeliveryGetVM>? DeliveryGetVM { get; set; }
    }
}
