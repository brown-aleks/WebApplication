using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApplicationJWT.Handlers;
using WebApplicationJWT.Models;

namespace WebApplicationJWT.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        public IConfiguration _configuration;
        private UserManager<User> _userManager;

        public AuthController(IConfiguration config, UserManager<User> userManager)
        {
            _configuration = config;
            _userManager = userManager;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequestErrorMessages();
            }

            var user = await _userManager.FindByEmailAsync(request.Email);
            var isAuthorized = user != null && await _userManager.CheckPasswordAsync(user, request.Password);

            if (isAuthorized)
            {
                var authResponse = await GetTokens(user);
                user.RefreshToken = authResponse.RefreshToken;
                await _userManager.UpdateAsync(user);
                return Ok(authResponse);
            }
            else
            {
                return BadRequest("Invalid credentials");
            }
        }

        [HttpGet("tokenValidate")]
        [Authorize]
        public async Task<IActionResult> TokenValidate()
        {
            //Эта конечная точка создана, чтобы любой пользователь мог проверить свой токен.
            return Ok("Token is valid");
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest registerRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequestErrorMessages();
            }

            var isEmailAlreadyRegistered = await _userManager.FindByEmailAsync(registerRequest.Email) != null;
            var isUserNameAlreadyRegistered = await _userManager.FindByNameAsync(registerRequest.Username) != null;

            if (isEmailAlreadyRegistered)
            {
                return Conflict($"Email {registerRequest.Email} is already registered.");
            }

            if (isUserNameAlreadyRegistered)
            {
                return Conflict($"User Name {registerRequest.Username} is already registered.");
            }

            var newUser = new User
            {
                Email = registerRequest.Email,
                UserName = registerRequest.Username,
                DisplayName = registerRequest.DisplayName
            };

            var result = await _userManager.CreateAsync(newUser,registerRequest.Password);

            if (result.Succeeded)
            {
                return Ok("User created successfully"); 
            }
            else
            {
                return StatusCode(500,result.Errors.Select(e => new{Msg = e.Code, Desc = e.Description}).ToList());
            }
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(RefreshRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequestErrorMessages();
            }

            //получаем электронную почту из строки токена с истекшим сроком действия
            var specialTokenHandler = new SpecialTokenHandler();
            var principal = specialTokenHandler.GetPrincipalFromToken(request.AccessToken, _configuration);
            var userEmail = principal.FindFirstValue("Email"); //получаем значение claim по электронной почте

            //проверяем, существует ли пользователь с такой электронной почтой, и соответствует ли токен обновления.
            var user = !string.IsNullOrEmpty(userEmail) ? await _userManager.FindByEmailAsync(userEmail) : null;
            if (user == null || user.RefreshToken != request.RefreshToken)
            {
                return BadRequest("Invalid refresh token");
            }

            //отдаём новые токены доступа и обновляем БД
            var response = await GetTokens(user);
            user.RefreshToken = response.RefreshToken;
            await _userManager.UpdateAsync(user);
            return Ok(response);
        }

        [HttpPost("revoke")]
        [Authorize]
        public async Task<IActionResult> Revoke(RevokeRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequestErrorMessages();
            }

            //получаем электронную почту из утверждений текущего пользователя, вошедшего в систему
            var userEmail = HttpContext.User.FindFirstValue("Email");

            //проверяем, существует ли пользователь с такой электронной почтой, и соответствует ли токен обновления.
            var user = !string.IsNullOrEmpty(userEmail) ? await _userManager.FindByEmailAsync(userEmail) : null;
            if (user == null || user.RefreshToken != request.RefreshToken)
            {
                return BadRequest("Invalid refresh token");
            }

            //удаляем RefreshToken 
            user.RefreshToken = null;
            await _userManager.UpdateAsync(user);
            return Ok("Refresh token is revoked");
        }

        private async Task<AuthResponse> GetTokens(User user)
        {
            var specialTokenHandler = new SpecialTokenHandler();
            var accessToken = specialTokenHandler.GetAccessToken(user, _configuration);
            var tokenStr = specialTokenHandler.GetEncryptedString(accessToken, _configuration["token:key"]);
            var refreshTokenStr = specialTokenHandler.GetRefreshToken(_userManager);
            var authResponse = new AuthResponse { AccessToken = tokenStr, RefreshToken = refreshTokenStr };
            return await Task.FromResult(authResponse);
        }

        private IActionResult BadRequestErrorMessages()
        {
            var errMsgs = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage));
            return BadRequest(errMsgs);
        }
    }
}
