using System.ComponentModel.DataAnnotations;

namespace resetPasswordUsingOtp.Models
{
    public class LoginRequestModel
    {
        public required string UserName { get; set; }
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
