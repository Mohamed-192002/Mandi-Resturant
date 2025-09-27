namespace Core.ViewModels.DriverPriceVM
{
    public class DriverPriceMainVM
    {
        public DriverPriceRegisterVM DriverPriceRegisterVM { get; set; } = new DriverPriceRegisterVM();
        public List<DriverPriceGetVM>? DriverPriceGetVM { get; set; } = new List<DriverPriceGetVM>();
    }
}
