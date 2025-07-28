using Core.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.ViewModels.HoleVM
{
    public class MeatHoleDataVM
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public HoleType HoleType { get; set; }
        public TimeOnly? StartTime { get; set; }
        public TimeOnly? EndTime { get; set; }
        public int NafrAmount { get; set; }
        public int HalfNafrAmount { get; set; }
    }
}
