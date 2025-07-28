using Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.ViewModels.DeliveryBillVM
{
    public class DriverBillReportVM
    {
        public int? DriverId { get; set; }
        public List<CommonDrop>? Drivers { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public List<DeliveryBillGetVM> DeliveryBillGetVM { get; set; } = new List<DeliveryBillGetVM>();
        public double TotalPaid { get; set; }
        public double TotalUnPaid { get; set; }
    }
}
