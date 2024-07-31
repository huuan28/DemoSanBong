using DemoSanBong.Models;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Core.Types;
using System.ComponentModel.DataAnnotations;

namespace DemoSanBong.ViewModels
{
    public class BookingViewModel
    {
        public int? Id { get; set; }

        [Display(Name = "Hình thức thuê")]
        public int RentalType { get; set; }

        public DateTime SelectedDate { get; set; }

        [Display(Name = "Ngày nhận")]
        public DateTime? CheckinDate { get; set; }

        public DateTime? CreateDate { get; set; }


        [Display(Name = "Ngày trả")]
        public DateTime? CheckoutDate { get; set; }

        [Display(Name = "Tiền đặt cọc")]
        public double? Deposit { get; set; }

        public string PhoneNumber { get; set; }
        public string FullName { get; set; }

        public int? Status { get; set; }

        public List<FieldViewModel>? AvailableField {  get; set; }
        public SelectedFieldsViewModel? SelectedFields { get; set; }

        public List<DateTime>? SelectDay { get; set; }
        public List<DateTime>? SelectBegin { get; set; }
        public List<DateTime>? SelectEnd { get; set; }

        public AppUser? Customer { get; set; }

        public int? MonthNum { get; set; }

        public string? Type
        {
            get
            {
                switch (RentalType)
                {
                    case 0: return "Thuê giờ";
                    default: return "thuê tháng";
                }
            }
        }
        public string? State
        {
            get
            {
                switch (Status)
                {
                    case 1: return "Đã đặt cọc";
                    case 2: return "Đã thay đổi";
                    case 3: return "Đã hủy";
                    case 4: return "Đã nhận";
                    default: return "Mới tạo";
                }
            }
        }
        public string? TextColor
        {
            get
            {
                switch (Status)
                {
                    case 1: return "text-info";
                    case 2: return "text-warning";
                    case 3: return "text-danger";
                    case 4: return "text-success";
                    default: return "text-primary";
                }
            }
        }
    }
}
