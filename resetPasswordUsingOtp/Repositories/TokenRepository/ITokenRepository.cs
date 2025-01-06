using Microsoft.AspNetCore.Identity;

namespace resetPasswordUsingOtp.Repositories.TokenRepository
{
    public interface ITokenRepository
    {
        string CreateJWTToken(IdentityUser user, List<string> roles);
    }
}
