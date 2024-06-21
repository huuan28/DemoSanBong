using System.ComponentModel.DataAnnotations;

namespace DemoSanBong.Models
{
    public class Invoice
    {
        [Key, Display(Name = "Mã hóa đơn")]
        public int Id { get; set; }

        [Required, Display(Name = "Ngày tạo")]
        public DateTime CreateDate { get; set; }

        [Required, Display(Name = "Ngày nhận dự kiến")]
        public DateTime CheckinDate { get; set; }

        [Required, Display(Name = "Ngày trả dự kiến")]
        public DateTime CheckoutDate { get; set; }

        [Required, Display(Name = "Tổng cộng")]
        public double Amount { get; set; }

        [Required, Display(Name = "Hình thức thanh toán")]
        public int PaymentMethod { get; set; }

        [Required, Display(Name = "Trạng thái")]
        public int Status { get; set; }

        [Required, Display(Name ="Mã đặt sân")]
        public int BookingId { get; set; }
        public Booking Booking { get; set; }


        public string CashierId { get; set; }
        public AppUser Cashier { get; set; }

        public int VAT { get; set; }
        public string Note { get; set; }

        public static readonly int CheckedIn = 0;
        public static readonly int CheckedOut = 1;
        public static readonly int Paid = 2;

    }
}
