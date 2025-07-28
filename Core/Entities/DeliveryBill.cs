using Core.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class DeliveryBill:EntityBase
    {
        public int DeliveryId { get; set; }

        [ForeignKey(nameof(DeliveryId))]
        public Delivery Delivery { get; set; }
        public int? CustomerId { get; set; }

        [ForeignKey(nameof(CustomerId))]
        public Customer Customer { get; set; }
        public int BillNumber { get; set; }
        public DateTime Date { get; set; }
        public double TotalOrder { get; set; }
        public bool Delivered { get; set; }
    }
}
