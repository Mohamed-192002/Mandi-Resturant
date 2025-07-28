namespace Core.ViewModels.MeatFillingVM
{
    public class MeatFillingGetVM
    {
        public int Id { get; set; }
        public string? HoleName { get; set; }
        public DateTime Date { get; set; }
        public int Nafr { get; set; }
        public int HalfNafr { get; set; }
        public TimeOnly EndTime { get; set; }
    }
}
