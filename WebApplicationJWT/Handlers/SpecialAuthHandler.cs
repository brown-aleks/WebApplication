//  https://memorycrypt.hashnode.dev/create-a-web-api-with-jwt-authentication-and-aspnet-core-identity

using WebApplicationJWT.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.Net.Http.Headers;

namespace WebApplicationJWT.Handlers
{
    public class SpecialAuthHandler : AuthenticationHandler<SpecialAuthenticationSchemeOptions>
    {
        private IConfiguration _config;
        private UserManager<User> _userManager;

        public SpecialAuthHandler(
            IOptionsMonitor<SpecialAuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IConfiguration config,
            UserManager<User> userManager)
            : base(options, logger, encoder, clock)
        {
            _config = config;
            _userManager = userManager;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            try
            {
                if (!Request.Headers.ContainsKey(HeaderNames.Authorization))
                {
                    return AuthenticateResult.Fail("Header Not Found.");
                }

                var header = Request.Headers[HeaderNames.Authorization].ToString();

                string tokenString = header.Substring("bearer".Length).Trim();

                var key = _config["token:key"];
                var tokenHandler = new SpecialTokenHandler();

                var token = tokenHandler.GetDecryptedToken(tokenString, key);
                var email = token.Claims["Email"];

                var isTokenExpired = token.ValidTo < DateTime.Now;
                var userNotExist = await _userManager.FindByEmailAsync(email) == null;

                if (isTokenExpired || userNotExist)
                {
                    return AuthenticateResult.Fail($"Unauthorized");
                }

                var principal = tokenHandler.GetPrincipalFromToken(tokenString, _config);   //если пользователь аутентифицирован, управление перейдет сюда

                var ticket = new AuthenticationTicket(principal, this.Scheme.Name);     // создаём AuthenticationTicket от Identity и действующей схемы аутентификации

                return AuthenticateResult.Success(ticket);  // передаём ticket промежуточному программному обеспечению
            }
            catch (Exception ex)
            {
                return AuthenticateResult.Fail($"Unauthorized: {ex.Message}");
            }
        }
    }
}