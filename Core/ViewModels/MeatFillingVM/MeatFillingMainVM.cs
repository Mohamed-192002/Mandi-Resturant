namespace Core.ViewModels.MeatFillingVM
{
    public class MeatFillingMainVM
    {
        public MeatFillingRegisterVM MeatFillingRegisterVM { get; set; } = new MeatFillingRegisterVM();
        public List<MeatFillingGetVM>? MeatFillingGetVM { get; set; }
    }
}
