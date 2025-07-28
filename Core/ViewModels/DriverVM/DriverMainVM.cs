namespace Core.ViewModels.DriverVM
{
    public class DriverMainVM
    {
        public DriverRegisterVM DriverRegisterVM { get; set; } = new DriverRegisterVM();
        public List<DriverGetVM>? DriverGetVM { get; set; } = new List<DriverGetVM>();
    }
}
