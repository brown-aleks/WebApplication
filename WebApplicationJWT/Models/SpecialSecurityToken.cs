//  https://memorycrypt.hashnode.dev/create-a-web-api-with-jwt-authentication-and-aspnet-core-identity

using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text.Json.Serialization;

namespace WebApplicationJWT.Models
{
    public class SpecialSecurityToken : SecurityToken
    {
        private string _id, _issuer, _audience;
        private DateTime _validFrom, _validTo;
        private Dictionary<string, string> _claims;
        public SpecialSecurityToken(string issuer, string audience, Dictionary<string, string> claims, DateTime expires)
        {
            _issuer = issuer;
            _audience = audience;
            _claims = claims;
            _validFrom = DateTime.Now;
            _validTo = expires;
        }
        [JsonIgnore]
        public override string Id { get { return _id; } }
        public override string Issuer { get { return _issuer; } }
        [JsonIgnore]
        public override SecurityKey SecurityKey { get; }        //  не использую
        [JsonIgnore]
        public override SecurityKey SigningKey { get; set; }    //  не использую
        [JsonIgnore]
        public override DateTime ValidFrom { get { return _validFrom; } }
        public override DateTime ValidTo { get { return _validTo; } }
        public string Audience { get { return _audience; } }
        public Dictionary<string, string> Claims { get { return _claims; } }
    }
}