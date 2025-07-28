using Core.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class ChickenFilling:EntityBase
    {
        public int HoleId { get; set; }

        [ForeignKey(nameof(HoleId))]
        public Hole Hole { get; set; }
        public DateTime Date { get; set; }
        public double Amount { get; set; }
        public TimeOnly EndTime { get; set; }
    }
}
