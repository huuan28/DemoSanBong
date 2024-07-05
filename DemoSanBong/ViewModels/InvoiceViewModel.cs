using DemoSanBong.Models;
using System.ComponentModel.DataAnnotations;

namespace DemoSanBong.ViewModels
{
    public class InvoiceViewModel
    {
        [Display(Name = "Mã hóa đơn")]
        public int? Id { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime? CreateDate { get; set; }

        [Display(Name = "Ngày nhận dự kiến")]
        public DateTime? CheckinDate { get; set; }

        [Display(Name = "Ngày trả dự kiến")]
        public DateTime? CheckoutDate { get; set; }

        [Display(Name = "Tổng cộng")]
        public double? Amount { get; set; }

        [Display(Name = "Hình thức thanh toán")]
        public int? PaymentMethod { get; set; }

        [Display(Name = "Trạng thái")]
        public int Status { get; set; }

        [Display(Name = "Mã đặt sân")]
        public Booking Booking { get; set; }

        public AppUser? Cashier { get; set; }

        public string? Note { get; set; }

        public List<FieldViewModel>? Fields { get; set; }

        public List<Service>? Services { get; set; }

        public List<InvoiceService>? Order {  get; set; }
    }
}
