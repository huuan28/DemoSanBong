using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace DemoSanBong.Models
{
    public class AppUser : IdentityUser
    {
        [Required]
        [Display(Name ="Họ và Tên")]
        public string FullName { get; set; }

        [Display(Name ="Cấp bậc")]
        public int? Level { get; set; } //VIP1-VIP2 or Basic-Premium
        public CustomerLevel CustomerLevel { get; set; }

        [Display(Name ="Ảnh đại diện")]
        public string? Avatar { get; set; } //link/path ảnh

        [Display(Name ="Mã tìm kiếm")]
        public string? SearchKey { get; set; } //mã dùng để tìm kiếm khi gõ KH....../ NV......

        public DateTime? BirthDate { get; set; }

        public bool? Gender { get; set; }

        public string? Address { get; set; }

        public DateTime? CreateDate { get; set; }

        [Display(Name ="Trạng thái")]
        public bool IsRegisted { get; set; } //khách đặt sân không đăng nhập và chưa đăng ký thì = false (tự tạo tk cho khách)

        public DateTime? ResgiterDate { get; set; }
    }
}
