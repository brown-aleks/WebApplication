//  https://memorycrypt.hashnode.dev/create-a-web-api-with-jwt-authentication-and-aspnet-core-identity

using System.ComponentModel.DataAnnotations;

namespace WebApplicationJWT.Models
{
    public class RevokeRequest
    {
        [Required]
        public string RefreshToken { get; set; }
    }

}