using Core.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities
{
    public class MeatFilling : EntityBase
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
