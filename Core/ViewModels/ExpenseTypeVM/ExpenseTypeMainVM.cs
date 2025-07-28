namespace Core.ViewModels.ExpenseTypeVM
{
    public class ExpenseTypeMainVM
    {
        public ExpenseTypeRegisterVM ExpenseTypeRegisterVM { get; set; } = new ExpenseTypeRegisterVM();
        public List<ExpenseTypeGetVM>? ExpenseTypeGetVM { get; set; }
    }
}
