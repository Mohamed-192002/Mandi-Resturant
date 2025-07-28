using Core.Common;
using Core.Common.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class ChickenHoleMovement:EntityBase
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
