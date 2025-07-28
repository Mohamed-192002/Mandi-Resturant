namespace Core.ViewModels.ChickenFillingVM
{
    public class ChickenFillingMainVM
    {
        public ChickenFillingRegisterVM ChickenFillingRegisterVM { get; set; } = new ChickenFillingRegisterVM();
        public List<ChickenFillingGetVM>? ChickenFillingGetVM { get; set; }
    }
}
