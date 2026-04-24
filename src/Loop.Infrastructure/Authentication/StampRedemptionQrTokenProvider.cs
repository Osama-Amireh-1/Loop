using System.Security.Claims;
using System.Text;
using Loop.Application.Abstractions.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Loop.Infrastructure.Authentication;

internal sealed class StampRedemptionQrTokenProvider(IConfiguration configuration) : IStampRedemptionQrTokenProvider
{
    private static readonly JsonWebTokenHandler Handler = new();

    public string CreateToken(StampRedemptionQrTokenPayload payload)
    {
        string secretKey = configuration["Jwt:Secret"]!;
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new Claim(JwtRegisteredClaimNames.Jti, payload.TokenId),
                new Claim("stamp_id", payload.StampId.ToString()),
                new Claim("user_id", payload.UserId.ToString()),
                new Claim("shop_id", payload.ShopId.ToString())
            ]),
            Expires = payload.ExpiresAtUtc,
            SigningCredentials = credentials,
            Issuer = configuration["Jwt:Issuer"],
            Audience = configuration["Jwt:Audience"]
        };

        return Handler.CreateToken(tokenDescriptor);
    }

    public async Task<StampRedemptionQrTokenPayload?> ValidateAndGetPayloadAsync(string token)
    {
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]!)),
            ValidateIssuer = true,
            ValidIssuer = configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        var validationResult = await Handler.ValidateTokenAsync(token, validationParameters);

        if (!validationResult.IsValid || validationResult.ClaimsIdentity is null)
        {
            return null;
        }

        string? tokenId = validationResult.ClaimsIdentity.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
        string? stampId = validationResult.ClaimsIdentity.FindFirst("stamp_id")?.Value;
        string? userId = validationResult.ClaimsIdentity.FindFirst("user_id")?.Value;
        string? shopId = validationResult.ClaimsIdentity.FindFirst("shop_id")?.Value;

        if (string.IsNullOrWhiteSpace(tokenId)
            || !Guid.TryParse(stampId, out Guid parsedStampId)
            || !Guid.TryParse(userId, out Guid parsedUserId)
            || !Guid.TryParse(shopId, out Guid parsedShopId))
        {
            return null;
        }

        DateTime expiresAtUtc = validationResult.SecurityToken.ValidTo;

        return new StampRedemptionQrTokenPayload(
            tokenId,
            parsedStampId,
            parsedUserId,
            parsedShopId,
            expiresAtUtc);
    }
}
