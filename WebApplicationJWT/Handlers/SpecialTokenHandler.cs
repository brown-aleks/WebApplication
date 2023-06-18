//  https://memorycrypt.hashnode.dev/create-a-web-api-with-jwt-authentication-and-aspnet-core-identity

using System.Security.Cryptography;
using System.Text.Json;
using System.Text;
using WebApplicationJWT.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace WebApplicationJWT.Handlers
{
    public class SpecialTokenHandler
    {
        public string GetEncryptedString(SpecialSecurityToken token, string key)
        {
            var jsonSerializedToken = JsonSerializer.Serialize(token);
            byte[] iv = new byte[16];
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using MemoryStream memoryStream = new();
                using CryptoStream cryptoStream = new(memoryStream, encryptor, CryptoStreamMode.Write);
                using (StreamWriter streamWriter = new(cryptoStream))
                {
                    streamWriter.Write(jsonSerializedToken);
                }
                array = memoryStream.ToArray();
            }
            return Convert.ToBase64String(array);
        }

        public SpecialSecurityToken GetDecryptedToken(string tokenString, string key)
        {
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(tokenString);

            using Aes aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(key);
            aes.IV = iv;
            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using MemoryStream memoryStream = new(buffer);
            using CryptoStream cryptoStream = new(memoryStream, decryptor, CryptoStreamMode.Read);
            using StreamReader streamReader = new(cryptoStream);
            var jsonTokenString = streamReader.ReadToEnd();
            var tokenParams = JsonSerializer.Deserialize<Dictionary<string, dynamic>>(jsonTokenString);
            var issuer = tokenParams["Issuer"].ToString();
            var audience = tokenParams["Audience"].ToString();
            var validTo = JsonSerializer.Deserialize<DateTime>(tokenParams["ValidTo"]);
            var claims = JsonSerializer.Deserialize<Dictionary<string, string>>(tokenParams["Claims"]);
            var token = new SpecialSecurityToken(issuer, audience, claims, validTo);
            return token;
        }

        public ClaimsPrincipal GetPrincipalFromToken(string tokenString, IConfiguration config)
        {
            var key = config["token:key"];
            var token = GetDecryptedToken(tokenString, key);
            var claims = token.Claims.Select(c => new Claim(c.Key, c.Value)).ToList();      // создаём claims array из словаря в лист
            var claimsIdentity = new ClaimsIdentity(claims, nameof(SpecialTokenHandler));   // создаём claimsIdentity по названию класса
            var principal = new ClaimsPrincipal(claimsIdentity);                            // создаём principal

            return principal;
        }

        public SpecialSecurityToken GetAccessToken(User user, IConfiguration config)
        {
            //создание claims на основе информации о пользователе
            var claims = new[] {
                        new Claim("Subject", config["token:subject"]),
                        new Claim("Id", Guid.NewGuid().ToString()),
                        new Claim("Iat", DateTime.UtcNow.ToString()),
                        new Claim("UserId", user.Id),
                        new Claim("UserName", user.UserName),
                        new Claim("Email", user.Email)
                    };
            var claimsDictionary = claims.ToDictionary(c => c.Type, c => c.Value);
            var key = config["token:key"];
            var token = new SpecialSecurityToken(
                config["token:issuer"],
                config["token:audience"],
                claimsDictionary,
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(config["token:accessTokenExpiryMinutes"])));

            return token;
        }

        public string GetRefreshToken(UserManager<User> userManager)
        {
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var tokenIsUnique = !userManager.Users.Any(u => u.RefreshToken == token);   //  проверяем что токен уникален, проверив db

            if (!tokenIsUnique)
                return GetRefreshToken(userManager);    //  рекурсивный вызов

            return token;
        }
    }
}