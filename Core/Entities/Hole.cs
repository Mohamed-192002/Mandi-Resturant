using Core.Common;
using Core.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class Hole:EntityBaseName
    {
        public HoleType HoleType { get; set; }
    }
}
