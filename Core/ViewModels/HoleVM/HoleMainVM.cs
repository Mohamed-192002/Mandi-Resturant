using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.ViewModels.HoleVM
{
    public class HoleMainVM
    {
        public HoleRegisterVM HoleRegisterVM { get; set; } = new HoleRegisterVM();
        public List<HoleGetVM>? HoleGetVM { get; set; } = new List<HoleGetVM>();
    }
}
