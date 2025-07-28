using Core.Common.Enums;

namespace Core.ViewModels.HoleVM
{
    public class MeatHoleDataVM
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public HoleType HoleType { get; set; }
        public TimeOnly? StartTime { get; set; }
        public TimeOnly? EndTime { get; set; }
        public int NafrAmount { get; set; }
        public int HalfNafrAmount { get; set; }
    }
}
