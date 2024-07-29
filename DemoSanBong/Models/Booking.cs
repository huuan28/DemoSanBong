using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DemoSanBong.Models
{
    public class Booking
    {
        [Key, Display(Name ="Mã đặt sân")]
        public int Id { get; set; }

        [Display(Name = "Hình thức thuê")]
        public int RentalType { get; set; }

        [ Display(Name ="Ngày tạo")]
        public DateTime CreateDate { get; set; }

        [Display(Name = "Ngày nhận")]
        public DateTime? CheckinDate { get; set; }

        [Display(Name = "Ngày trả")]
        public DateTime? CheckoutDate { get; set; }

        public int PaymentGate { get; set; }

        [Display(Name = "Tiền đặt cọc")]
        public double Deposit { get; set; }

        [Display(Name = "Trạng thái")]
        public int Status { get; set; }

        //[ForeignKey("Customer")]
        public string? CusID { get; set; }
        public AppUser Customer { get; set; }
        public static readonly int RentByHour = 0; //thuê theo giờ
        public static readonly int RentByMonth = 1; //thuê theo tháng
        //status
        public static readonly int Deposited = 1; //chưa nhận
        public static readonly int Changed = 2; //đã đổi
        public static readonly int Cancelled = 3; //đã hủy
        public static readonly int CheckedIn = 4; //đã nhận
        public static readonly int CheckedOut = 4; //đã trả
        public static readonly int TimeOut = 4; //quá hạn
    }
}
