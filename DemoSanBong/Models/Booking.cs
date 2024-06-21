using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DemoSanBong.Models
{
    public class Booking
    {
        [Key, Display(Name ="Mã đặt sân")]
        public int Id { get; set; }

        [Required, Display(Name = "Hình thức thuê")]
        public int RentalType { get; set; }

        [Required, Display(Name ="Ngày tạo")]
        public DateTime CreateDate { get; set; }

        [Required, Display(Name = "Ngày nhận dự kiến")]
        public DateTime CheckinDate { get; set; }

        [Required, Display(Name = "Ngày trả dự kiến")]
        public DateTime CheckoutDate { get; set; }

        [Required]
        public int PaymentMethod { get; set; }

        [Required, Display(Name = "Tiền đặt cọc")]
        public double Deposit { get; set; }

        [Required, Display(Name = "Trạng thái")]
        public int Status { get; set; }

        //[ForeignKey("Customer")]
        public string? CusID { get; set; }
        public AppUser Customer { get; set; }





        public static readonly int RentByHour = 0;
        public static readonly int RentByMonth = 1;

        public static readonly int Deposited = 1;
        public static readonly int Changed = 2;
        public static readonly int Cancelled = 3;
        public static readonly int CheckedIn = 4;
    }
}
