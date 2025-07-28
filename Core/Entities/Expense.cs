using Core.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class Expense:EntityBase
    {
        public int ExpenseTypeId { get; set; }

        [ForeignKey(nameof(ExpenseTypeId))]
        public ExpenseType ExpenseType { get; set; }
        public double Payment { get; set; }
        public DateTime Date { get; set; }
        public string? Notes { get; set; }
    }
}
