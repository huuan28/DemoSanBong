namespace DemoSanBong.Models
{
    public class FieldRate
    {
        public double Price { get; set; }
        public DateTime EffectiveDate { get; set; }
        public int Type { get; set; }
        public int FieldId { get; set; }
        public Field Field { get; set; }
    }
}
