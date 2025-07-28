using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.ViewModels.ExpenseTypeVM
{
    public class ExpenseTypeMainVM
    {
        public ExpenseTypeRegisterVM ExpenseTypeRegisterVM { get; set; } = new ExpenseTypeRegisterVM();
        public List<ExpenseTypeGetVM>? ExpenseTypeGetVM { get; set; }
    }
}
