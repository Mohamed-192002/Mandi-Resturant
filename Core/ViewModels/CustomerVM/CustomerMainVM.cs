using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.ViewModels.CustomerVM
{
    public class CustomerMainVM
    {
        public CustomerRegisterVM CustomerRegisterVM { get; set; } = new CustomerRegisterVM();
        public List<CustomerGetVM>? CustomerGetVM { get; set; }
    }
}
