using System.ComponentModel.DataAnnotations;

namespace DemoSanBong.Models
{
    public class ServiceRate
    {
        
        public int ServiceId { get; set; }
        public Service Service { get; set; }
        public DateTime EffectiveDate { get; set; }
        public double Price { get; set; }
    }
}
