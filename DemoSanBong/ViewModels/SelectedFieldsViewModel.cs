namespace DemoSanBong.ViewModels
{
    public class SelectedFieldsViewModel
    {
        public int RentalType { get; set; }
        public int MonthNum { get; set; }
        public double Amount { get; set; }
        public double Deposit { get; set; }
        public List<FieldViewModel> SelectedFields { get; set; }
    }
}
