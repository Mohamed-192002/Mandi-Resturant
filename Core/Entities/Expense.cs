using Core.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities
{
    public class Expense : EntityBase
    {
        public int ExpenseTypeId { get; set; }

        [ForeignKey(nameof(ExpenseTypeId))]
        public ExpenseType ExpenseType { get; set; }
        public double Payment { get; set; }
        public DateTime Date { get; set; }
        public string? Notes { get; set; }
    }
}
