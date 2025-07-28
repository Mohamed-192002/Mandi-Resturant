using Core.Common.Enums;
using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.ViewModels.SaleBillVM
{
    public class SaleBillGetVM
    {
        public int Id { get; set; }
        public BillType BillType { get; set; }
        public int? CustomerId { get; set; }
        public int? DeliveryId { get; set; }
        public DateTime Date { get; set; }
        public double Total { get; set; }
        public double Discount { get; set; }
        public double Vat { get; set; }
        public double FinalTotal { get; set; }
        public bool Confirmed { get; set; } = true;
        public Guid? UserConfirmed { get; set; }
        public string? TableNames { get; set; }
    }
}
