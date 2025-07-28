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
    public class MeatHoleMovement:EntityBase
    {
        public int HoleId { get; set; }

        [ForeignKey(nameof(HoleId))]
        public Hole Hole { get; set; }
        public DateTime Date { get; set; }
        public double NafrAmountIn { get; set; }
        public double NafrAmountOut { get; set; }
        public double HalfNafrAmountIn { get; set; }
        public double HalfNafrAmountOut { get; set; }
        public HoleMovementType HoleMovementType { get; set; }
        public int HoleMovementTypeId { get; set; }
    }
}
