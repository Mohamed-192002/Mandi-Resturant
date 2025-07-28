using Core.Common;
using Core.Common.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities
{
    public class ChickenHoleMovement : EntityBase
    {
        public int HoleId { get; set; }

        [ForeignKey(nameof(HoleId))]
        public Hole Hole { get; set; }
        public DateTime Date { get; set; }
        public double AmountIn { get; set; }
        public double AmountOut { get; set; }
        public HoleMovementType HoleMovementType { get; set; }
        public int HoleMovementTypeId { get; set; }
    }
}
