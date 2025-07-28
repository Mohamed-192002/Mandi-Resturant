using Core.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.ViewModels.HoleVM
{
    public class HoleGetVM
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public HoleType HoleType { get; set; }
    }
}
