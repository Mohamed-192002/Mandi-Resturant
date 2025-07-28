using Core.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.ViewModels.SaleBillVM
{
    public class SaleBillRefundVM
    {
        public int Id { get; set; }
        public BillType BillType { get; set; }
        public List<BillDetailRegisterVM>? BillDetailRegisterVM { get; set; }
    }
}
