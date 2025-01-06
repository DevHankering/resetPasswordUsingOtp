using System.ComponentModel.DataAnnotations;

namespace resetPasswordUsingOtp.Models
{
    public class RegisterModel
    {
        public string UserName { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        [DataType(DataType.Password)]
        public string Password { get; set; }

        //[Compare("Passwordd", ErrorMessage = "The password and confirm password do not match.")]
        public string ConfirmPassword { get; set; }
        public string[] Roles { get; set; }
    }
}
