using System.ComponentModel.DataAnnotations;

namespace DemoSanBong.Models
{
    public class Invoice
    {
        [Key, Display(Name = "Mã hóa đơn")]
        public int Id { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime CreateDate { get; set; }

        [Display(Name = "Ngày nhận")]
        public DateTime CheckinDate { get; set; }

        [Display(Name = "Ngày trả")]
        public DateTime? CheckoutDate { get; set; }

        [Display(Name = "Tổng cộng")]
        public double Amount { get; set; }

        [Display(Name = "Hình thức thanh toán")]
        public int PaymentMethod { get; set; }

        [Display(Name = "Trạng thái")]
        public int Status { get; set; }

        [Display(Name ="Mã đặt sân")]
        public int BookingId { get; set; }
        public Booking Booking { get; set; }


        public string CashierId { get; set; }
        public AppUser Cashier { get; set; }

        public int VAT { get; set; }
        public string? Note { get; set; }

        public static readonly int CheckedIn = 0;
        public static readonly int CheckedOut = 1;
        public static readonly int Paid = 2;

    }
}
