using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.ViewModels.ProductVM
{
    public class ProductGetVM
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public double CostPrice { get; set; }
        public double SellingPrice { get; set; }
        public int Nafr { get; set; }
        public int HalfNafr { get; set; }
        public int Dagag { get; set; }
        public int HalfDagag { get; set; }
        public string? ImageUrl { get; set; }
    }
}
