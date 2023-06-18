//  https://memorycrypt.hashnode.dev/create-a-web-api-with-jwt-authentication-and-aspnet-core-identity

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace WebApplicationJWT.Models
{
    public class AuthDbContext : IdentityDbContext<User>
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }
    }
}