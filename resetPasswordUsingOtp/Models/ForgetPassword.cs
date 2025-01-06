using System.ComponentModel.DataAnnotations;

namespace resetPasswordUsingOtp.Models
{
    public class ForgetPassword
    {
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; }

        //[Required(ErrorMessage = "current password is required ")]
        //public string CurrentPassword { get; set; }

        [Required(ErrorMessage = " New password is required")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "New Confirm Password is required")]
        public string ConfirmPassword { get; set; }

        public string Otp { get; set; }
    }
}
