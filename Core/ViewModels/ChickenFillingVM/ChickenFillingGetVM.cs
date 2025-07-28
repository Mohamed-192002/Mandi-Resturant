namespace Core.ViewModels.ChickenFillingVM
{
    public class ChickenFillingGetVM
    {
        public int Id { get; set; }
        public string? HoleName { get; set; }
        public DateTime Date { get; set; }
        public double Amount { get; set; }
        public TimeOnly EndTime { get; set; }
    }
}
