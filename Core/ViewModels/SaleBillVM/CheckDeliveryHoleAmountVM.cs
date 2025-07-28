using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.ViewModels.SaleBillVM
{
    public class CheckDeliveryHoleAmountVM
    {
        public List<BillDetailRegisterVM>? BillDetailRegisterVM { get; set; }
        public List<string>? HoleIds { get; set; }
        public string? OrderDeliveredTime { get; set; }
    }
}
