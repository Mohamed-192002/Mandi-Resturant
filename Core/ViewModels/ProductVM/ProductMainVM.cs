using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.ViewModels.ProductVM
{
    public class ProductMainVM
    {
        public ProductRegisterVM ProductRegisterVM { get; set; } = new ProductRegisterVM();
        public List<ProductGetVM>? ProductGetVM { get; set; }
    }
}
