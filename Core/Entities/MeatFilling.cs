using Core.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class MeatFilling:EntityBase
    {
        public int HoleId { get; set; }

        [ForeignKey(nameof(HoleId))]
        public Hole Hole { get; set; }
        public DateTime Date { get; set; }
        public int Nafr { get; set; }
        public int HalfNafr { get; set; }
        public TimeOnly EndTime { get; set; }
    }
}
