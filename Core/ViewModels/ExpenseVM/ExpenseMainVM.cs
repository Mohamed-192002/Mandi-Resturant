using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.ViewModels.ExpenseVM
{
    public class ExpenseMainVM
    {
        public ExpenseRegisterVM ExpenseRegisterVM { get; set; } = new ExpenseRegisterVM();
        public List<ExpenseGetVM>? ExpenseGetVM { get; set; }
    }
}
