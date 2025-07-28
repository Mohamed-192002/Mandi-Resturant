namespace Core.ViewModels.ExpenseVM
{
    public class ExpenseMainVM
    {
        public ExpenseRegisterVM ExpenseRegisterVM { get; set; } = new ExpenseRegisterVM();
        public List<ExpenseGetVM>? ExpenseGetVM { get; set; }
    }
}
