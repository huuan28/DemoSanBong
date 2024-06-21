using System.ComponentModel.DataAnnotations;

namespace DemoSanBong.Models
{
    public class GuestBill
    {
        [Key]
        public int Id { get; set; }
        public DateTime CreateDate { get; set; }
        public string CashierId { get; set; }
        public AppUser Cashier { get; set; }
        public string? GuestPhoneNumber { get; set; }
    }
}
