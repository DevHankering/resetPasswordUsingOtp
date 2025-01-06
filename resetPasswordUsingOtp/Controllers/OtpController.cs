using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using resetPasswordUsingOtp.Data;
using resetPasswordUsingOtp.Models;
using resetPasswordUsingOtp.Repositories.TokenRepository;
using resetPasswordUsingOtp.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using Microsoft.AspNetCore.Authorization;

namespace resetPasswordUsingOtp.Controllers
{

  
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]

    public class OtpController : ControllerBase
    {
        private readonly OtpDbContext db;
        private readonly UserManager<IdentityUser> userManager;
        private readonly ITokenRepository tokenRepository;
        private readonly OtpService otpService;
        private readonly IEmailService emailService;
        private readonly IMemoryCache cache;

        public OtpController(OtpDbContext   _db, UserManager<IdentityUser> userManager,ITokenRepository tokenRepository, OtpService otpService, IEmailService emailService, IMemoryCache cache)
        {
            db = _db;
            this.userManager = userManager;
            this.tokenRepository = tokenRepository;
            this.otpService = otpService;
            this.emailService = emailService;
            this.cache = cache;
        }

        [HttpPost]
        [Route("RegisterUser")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Register(RegisterModel registerModel)
        {
            var identityUser = new IdentityUser()   
            {
                UserName = registerModel.UserName,
                Email = registerModel.Email,
            };

            if(string.Compare(registerModel.Password, registerModel.ConfirmPassword) != 0)
            {
                return BadRequest("password and confirmPassword do not match");
            }

           var result = await userManager.CreateAsync(identityUser, registerModel.Password);

            if(result.Succeeded)
            {
                //Add roles to this User
                if (registerModel.Roles != null && registerModel.Roles.Any())
                {
                    result = await userManager.AddToRolesAsync(identityUser, registerModel.Roles);

                    if (result.Succeeded)
                    {
                        return Ok("User registered successfully, Please log in");
                    }
                }
                
            }
            return BadRequest("Something went wrong, User could not register!");
        }

        [HttpPost]
        [Route("ChangePassword")]
        [Authorize(Roles = "Admin, User")]
        public async Task<IActionResult> ChangePassword(ChangePasswordModel changePasswordModel)
        {
            var user = await userManager.FindByNameAsync(changePasswordModel.Username);
            if (user == null)
            {
                return BadRequest("user could not be found");
            }
            if (string.Compare(changePasswordModel.NewPassword, changePasswordModel.ConfirmPassword) != 0)
            {
                return BadRequest("new password and confirm new password do not match");
            }

            var result = await userManager.ChangePasswordAsync(user, changePasswordModel.CurrentPassword, changePasswordModel.NewPassword);
            if (!result.Succeeded)
            {
                var errors = new List<string>();

                foreach (var error in result.Errors)
                {
                    errors.Add(error.Description);
                }

                return BadRequest("Internal server error");
            }

            return Ok("password changed successfully");
        }

        [HttpPost]
        [Route("Login")]
        //[Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> Login([FromBody] LoginRequestModel model)
        {
            var user = await userManager.FindByNameAsync(model.UserName);
            if(user != null)
            { 
                var checkPasswordResult = await userManager.CheckPasswordAsync(user, model.Password);
                if (checkPasswordResult)
                {
                    var roles = await userManager.GetRolesAsync(user);

                    if (roles != null)
                    {
                        //CreateToken
                        var jwtToken = tokenRepository.CreateJWTToken(user, roles.ToList());

                        var response = new LoginResponseModel
                        {
                            JwtToken = jwtToken
                        };

                        return Ok(response);
                    }
                }
            }

            return BadRequest("UserName or Password incorrect");

        }


        [HttpPost]
        [Route("RequestForOtp")]
        [Authorize(Roles = "Admin, User")]
        public async Task<IActionResult> RequestForOtp(RequestForOtp changePasswordModel)
        {
            var user = await userManager.FindByEmailAsync(changePasswordModel.Email);
            if (user == null)
            {
                return BadRequest("user could not be found");
            }

            // Generate OTP
            var otp = otpService.GenerateOtp();

            if (otp == null)
            {
                return BadRequest("otp is not created");
            }

            // Store OTP in memory cache (could also use database with expiration)
            cache.Set(changePasswordModel.Email, otp, TimeSpan.FromMinutes(10)); // OTP expires in 10 minutes

            // Send OTP email
            var subject = "Password Reset OTP";
            var body = $"Your OTP for resetting your password is: {otp}. This OTP is valid for 10 minutes.";

            await emailService.SendEmailAsync(changePasswordModel.Email, subject, body);

            return Ok("OTP sent successfully.");

        }


        [HttpPost]
        [Route("VerifyOtpAndChangePassword")]
        [Authorize(Roles = "Admin, User")]
        public async Task<IActionResult> ChangePasswordWithOtp(ResetPasswordWithOtp changePasswordModel)
        {
            var user = await userManager.FindByEmailAsync(changePasswordModel.Email);
            if (user == null)
            {
                return BadRequest("user could not be found");
            }


            // Verify OTP first
            if (cache.TryGetValue(changePasswordModel.Email, out string storedOtp))


            {
                if (storedOtp == changePasswordModel.Otp)
                {
                    // Proceed with password reset logic here
                    // For example, update the password in the database.


                    if (string.Compare(changePasswordModel.NewPassword, changePasswordModel.ConfirmPassword) != 0)
                    {
                        return BadRequest("new password and confirm new password do not match");
                    }


                    var result = await userManager.ChangePasswordAsync(user, changePasswordModel.CurrentPassword, changePasswordModel.NewPassword);
                    if (!result.Succeeded)
                    {
                        var errors = new List<string>();

                        foreach (var error in result.Errors)
                        {
                            errors.Add(error.Description);
                        }

                        return BadRequest("Internal server error");
                    }



                    // Clear OTP from cache after successful verification
                    cache.Remove(changePasswordModel.Email);

                    return Ok("Password reset successfully.");
                }
                return BadRequest("Invalid OTP.");
            }

            return BadRequest("OTP expired or not found.");




            

           
        }



        //If you only want to verify your OTP
        [HttpPost("verify")]
        [Authorize(Roles = "Admin, User")]
        public IActionResult VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            if (cache.TryGetValue(request.Email, out string storedOtp))
            {
                // Check if the OTP is correct
                if (storedOtp == request.Otp)
                {
                    return Ok("OTP verified successfully.");
                }
                return BadRequest("Invalid OTP.");
            }
            return BadRequest("OTP expired or not found.");
        }




        //change password if you forgot it
        [HttpPost]
        [Route("ForgetPassword")]
        [Authorize(Roles = "Admin, User")]
        public async Task<IActionResult> ForgetPassword(ForgetPassword changePasswordModel)
        {
            var user = await userManager.FindByEmailAsync(changePasswordModel.Email);
            if (user == null)
            {
                return BadRequest("user could not be found");
            }


            // Verify OTP first
            if (cache.TryGetValue(changePasswordModel.Email, out string storedOtp))


            {
                if (storedOtp == changePasswordModel.Otp)
                {
                    // Proceed with password reset logic here
                    // For example, update the password in the database.


                    if (string.Compare(changePasswordModel.NewPassword, changePasswordModel.ConfirmPassword) != 0)
                    {
                        return BadRequest("new password and confirm new password do not match");
                    }


                    //var result = await userManager.ChangePasswordAsync(user, user.PasswordHash, changePasswordModel.NewPassword);

                    // Generate a password reset token
                    var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);

                    // Use the token to reset the password
                    var resetPasswordResult = await userManager.ResetPasswordAsync(user, resetToken, changePasswordModel.NewPassword);


                    if (!resetPasswordResult.Succeeded)
                    {
                        var errors = new List<string>();

                        foreach (var error in resetPasswordResult.Errors)
                        {
                            errors.Add(error.Description);
                        }

                        return BadRequest("Internal server error");
                    }

                    // Clear OTP from cache after successful verification
                    cache.Remove(changePasswordModel.Email);

                    return Ok("Password reset successfully.");
                }
                return BadRequest("Invalid OTP.");
            }

            return BadRequest("OTP expired or not found.");


        }


        [HttpDelete]
        [Route("DeleteUser")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> DeleteUser(DeleteModel model)
        {
            var existingUser = await db.Users.FirstOrDefaultAsync(a => a.UserName == model.UserName);
            if (existingUser == null)
            {
                return NotFound("User not found!");
            }
            db.Users.Remove(existingUser);
            await db.SaveChangesAsync();
            return Ok(existingUser);
        }

        [HttpGet]
        [Route("GetAllUsers")]
        [Authorize(Roles = "User,Admin")]
        public async Task<ActionResult<List<User>>> GetAllData()
        {
            var users = await db.Users.ToListAsync();
            return Ok(users);
           
        }

    }


    public class OtpService
    {
        private readonly Random _random = new Random();

        // Generate a random OTP of 6 digits
        public string GenerateOtp()
        {
            return _random.Next(100000, 999999).ToString();
        }
    }

}
