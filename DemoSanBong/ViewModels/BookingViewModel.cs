using DemoSanBong.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace DemoSanBong.ViewModels
{
    public class BookingViewModel
    {
        [Display(Name = "Hình thức thuê")]
        public int RentalType { get; set; }

        public DateTime SelectedDate { get; set; }

        [Display(Name = "Ngày nhận dự kiến")]
        public DateTime CheckinDate { get; set; }

        [Display(Name = "Ngày trả dự kiến")]
        public DateTime CheckoutDate { get; set; }

        [Display(Name = "Tiền đặt cọc")]
        public double? Deposit { get; set; }

        public string PhoneNumber { get; set; }
        public string FullName { get; set; }

        public List<FieldViewModel>? AvailableField {  get; set; }
        public List<FieldViewModel>? SelectedFields { get; set; }

        public List<DateTime>? SelectDay { get; set; }
        public List<DateTime>? SelectBegin { get; set; }
        public List<DateTime>? SelectEnd { get; set; }
    }
}
