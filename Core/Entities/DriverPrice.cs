using Core.Common;

namespace Core.Entities
{
    public class DriverPrice : EntityBase
    {
        public string Region { get; set; } = string.Empty;
        public decimal DeliveryPrice { get; set; }
    }
}
