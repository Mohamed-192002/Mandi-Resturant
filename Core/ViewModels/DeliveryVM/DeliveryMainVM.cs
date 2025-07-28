namespace Core.ViewModels.DeliveryVM
{
    public class DeliveryMainVM
    {
        public DeliveryRegisterVM DeliveryRegisterVM { get; set; } = new DeliveryRegisterVM();
        public List<DeliveryGetVM>? DeliveryGetVM { get; set; }
    }
}
