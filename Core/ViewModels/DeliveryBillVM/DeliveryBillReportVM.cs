using Core.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.ViewModels.DeliveryBillVM
{
    public class DeliveryBillReportVM
    {
        public int? DeliveryId { get; set; }
        public List<CommonDrop>? Deliveries { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public List<DeliveryBillGetVM> DeliveryBillGetVM { get; set; } = new List<DeliveryBillGetVM>();
        public double TotalPaid { get; set; }
        public double TotalUnPaid { get; set; }
    }
}
