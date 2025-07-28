namespace Core.ViewModels.CustomerVM
{
    public class CustomerMainVM
    {
        public CustomerRegisterVM CustomerRegisterVM { get; set; } = new CustomerRegisterVM();
        public List<CustomerGetVM>? CustomerGetVM { get; set; }
    }
}
