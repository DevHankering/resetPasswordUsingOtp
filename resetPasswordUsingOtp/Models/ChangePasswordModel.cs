using System.ComponentModel.DataAnnotations;

namespace resetPasswordUsingOtp.Models
{
    public class ChangePasswordModel
    {
        [Required(ErrorMessage = "username is required")]
        public string Username { get; set; }

        [Required(ErrorMessage = "current password is required ")]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = " New password is required")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "New Confirm Password is required")]
        public string ConfirmPassword { get; set; }
    }
}
