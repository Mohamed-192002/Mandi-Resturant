using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.ViewModels.DriverVM
{
    public class DriverMainVM
    {
        public DriverRegisterVM DriverRegisterVM { get; set; } = new DriverRegisterVM();
        public List<DriverGetVM>? DriverGetVM { get; set; } = new List<DriverGetVM>();
    }
}
