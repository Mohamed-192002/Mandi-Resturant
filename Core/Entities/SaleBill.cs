using Core.Common;
using Core.Common.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities
{
    public class SaleBill : EntityBase
    {
        public BillType BillType { get; set; }
        public int? CustomerId { get; set; }

        [ForeignKey(nameof(CustomerId))]
        public Customer Customer { get; set; }
        public int? DeliveryId { get; set; }

        [ForeignKey(nameof(DeliveryId))]
        public Delivery Delivery { get; set; }

        public int? DriverId { get; set; }

        [ForeignKey(nameof(DriverId))]
        public Driver Driver { get; set; }
        public DateTime Date { get; set; }
        public double Total { get; set; }
        public double Discount { get; set; }
        public double Vat { get; set; }
        public double FinalTotal { get; set; }
        public double DeliveryPrice { get; set; }

        public bool MoneyDelivered { get; set; } = true;
        public Guid? UserDelivered { get; set; }
        public DateTime? DateDelivered { get; set; }
        public string? Notes { get; set; }
        public bool Temporary { get; set; } = false;
        public bool Gift { get; set; } = false;
        public TimeOnly? OrderDeliveredTime { get; set; }
        public string? CustomerAddress { get; set; }
        public string? OrderNumber { get; set; }
    }
}
