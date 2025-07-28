using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.ViewModels.ChickenFillingVM
{
    public class ChickenFillingMainVM
    {
        public ChickenFillingRegisterVM ChickenFillingRegisterVM { get; set; } = new ChickenFillingRegisterVM();
        public List<ChickenFillingGetVM>? ChickenFillingGetVM { get; set; }
    }
}
