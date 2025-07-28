using Core.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class SaleBillDetail:EntityBase
    {
        public int SaleBillId { get; set; }

        [ForeignKey(nameof(SaleBillId))]
        public SaleBill SaleBill { get; set; }
        public int ProductId { get; set; }
        public double Price { get; set; }
        public int Amount { get; set; }
        public double Discount { get; set; }
        public double TotalPrice { get; set; }
    }
}
