using Core.Common.Enums;

namespace Core.ViewModels.HoleVM
{
    public class HoleGetVM
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public HoleType HoleType { get; set; }
    }
}
