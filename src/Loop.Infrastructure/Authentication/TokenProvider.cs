using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Loop.Application.Abstractions.Authentication;
using Loop.Domain.Shops;
using Loop.Domain.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Loop.Infrastructure.Authentication;

internal sealed class TokenProvider(IConfiguration configuration) : ITokenProvider
{
    public string CreateAccessToken(User user)
    {
        return CreateAccessToken(
            subjectId: user.UserId.ToString(),
            email: user.Email.Value,
            shopAdminId: null,
            shopId: null);
    }

    public string CreateAccessToken(ShopAdmin shopAdmin)
    {
        return CreateAccessToken(
            subjectId: shopAdmin.ShopAdminId.ToString(),
            email: shopAdmin.Email.Value,
            shopAdminId: shopAdmin.ShopAdminId,
            shopId: shopAdmin.ShopId);
    }

    public (string RefreshToken, DateTime ExpiresAtUtc) CreateRefreshToken()
    {
        string refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        int refreshTokenExpirationInDays = configuration.GetValue<int>("Jwt:RefreshTokenExpirationInDays");

        return (refreshToken, DateTime.UtcNow.AddDays(refreshTokenExpirationInDays));
    }

    private string CreateAccessToken(string subjectId, string email, Guid? shopAdminId, Guid? shopId)
    {
        string secretKey = configuration["Jwt:Secret"]!;
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        List<Claim> claims =
        [
            new Claim(JwtRegisteredClaimNames.Sub, subjectId),
            new Claim(ClaimTypes.NameIdentifier, subjectId),
            new Claim(JwtRegisteredClaimNames.Email, email)
        ];

        if (shopAdminId is not null)
        {
            claims.Add(new Claim("shop_admin_id", shopAdminId.Value.ToString()));
        }

        if (shopId is not null)
        {
            claims.Add(new Claim("shop_id", shopId.Value.ToString()));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(configuration.GetValue<int>("Jwt:ExpirationInMinutes")),
            SigningCredentials = credentials,
            Issuer = configuration["Jwt:Issuer"],
            Audience = configuration["Jwt:Audience"]
        };

        var handler = new JsonWebTokenHandler();

        return handler.CreateToken(tokenDescriptor);
    }
}

