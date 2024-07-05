using NuGet.Protocol;

namespace DemoSanBong.Models
{
    public class InvoiceService
    {
        public int InvoiceId { get; set; }
        public Invoice Invoice { get; set; }
        public int ServiceId { get; set; }
        public Service Service { get; set; }
        public int Quantity { get; set; }
        public DateTime OrderDate { get; set; }
    }
}
