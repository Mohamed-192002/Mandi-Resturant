using Core.Common;

namespace Core.Entities
{
    public class Customer : EntityBaseName
    {
        public string Phone { get; set; } = string.Empty;
        public string? AnotherPhone { get; set; }
        public string Address { get; set; } = string.Empty;
        public string? Address2 { get; set; }
        public string? Address3 { get; set; }
        public string? Address4 { get; set; }

    }
}
