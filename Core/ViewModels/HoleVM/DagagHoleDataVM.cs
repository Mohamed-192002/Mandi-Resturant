using Core.Common.Enums;

namespace Core.ViewModels.HoleVM
{
    public class DagagHoleDataVM
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public HoleType HoleType { get; set; }
        public TimeOnly? StartTime { get; set; }
        public TimeOnly? EndTime { get; set; }
        public double Amount { get; set; }
    }
}
