using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.ViewModels.MeatFillingVM
{
    public class MeatFillingMainVM
    {
        public MeatFillingRegisterVM MeatFillingRegisterVM { get; set; } = new MeatFillingRegisterVM();
        public List<MeatFillingGetVM>? MeatFillingGetVM { get; set; }
    }
}
