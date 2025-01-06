 using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace resetPasswordUsingOtp.Data
{
    public class OtpDbContext : IdentityDbContext
    {
        public OtpDbContext(DbContextOptions<OtpDbContext> options) : base(options)
        {
                
        }

        //Data Seeding 

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            var adminRoleId = "87d2ef13-de5e-4877-aedc-75df20cad5a2";
            var userRoleId = "2ea26a0b-f22d-45e8-af3a-1f34dcab7486";

            var roles = new List<IdentityRole>
                        {
                            new IdentityRole
                            {
                                 Id = adminRoleId,    //this id is a string but since we are using Guid, we will convert it into an string
                                 ConcurrencyStamp = adminRoleId,
                                 Name = "Admin",          // this is name of this role
                                 NormalizedName = "Admin".ToUpper()
                            },
                            new IdentityRole
                            {
                                Id = userRoleId,
                                ConcurrencyStamp = userRoleId,
                                Name = "User",
                                NormalizedName = "User".ToUpper()
                            }
                        };

            // Now we will seed this inside the builder object
            builder.Entity<IdentityRole>().HasData(roles);

        }
    }
}
