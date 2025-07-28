namespace Core.ViewModels.ProductVM
{
    public class ProductMainVM
    {
        public ProductRegisterVM ProductRegisterVM { get; set; } = new ProductRegisterVM();
        public List<ProductGetVM>? ProductGetVM { get; set; }
    }
}
