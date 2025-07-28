using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.ViewModels.ExpenseVM
{
    public class ExpenseGetVM
    {
        public int Id { get; set; }
        public string? ExpenseTypeName { get; set; }
        public double Payment { get; set; }
        public DateTime Date { get; set; }
        public string? Notes { get; set; }
    }
}
