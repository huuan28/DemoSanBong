using System.ComponentModel.DataAnnotations;

namespace DemoSanBong.ViewModels
{
    public class RegisterViewModel
    {
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; }

        [Phone]
        [Display(Name = "Số điện thoại")]
        public string PhoneNumber { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set;}

    }

}
