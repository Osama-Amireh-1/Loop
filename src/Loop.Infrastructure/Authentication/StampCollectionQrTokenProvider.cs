using System.Globalization;
using System.Security.Claims;
using System.Text;
using Loop.Application.Abstractions.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Loop.Infrastructure.Authentication;

internal sealed class StampCollectionQrTokenProvider(IConfiguration configuration) : IStampCollectionQrTokenProvider
{
    private static readonly JsonWebTokenHandler Handler = new();

    public string CreateToken(StampCollectionQrTokenPayload payload)
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
                new Claim("shop_id", payload.ShopId.ToString()),
                new Claim("stamps_count", payload.StampsCount.ToString(CultureInfo.InvariantCulture))
            ]),
            Expires = payload.ExpiresAtUtc,
            SigningCredentials = credentials,
            Issuer = configuration["Jwt:Issuer"],
            Audience = configuration["Jwt:Audience"]
        };

        return Handler.CreateToken(tokenDescriptor);
    }

    public async Task<StampCollectionQrTokenPayload?> ValidateAndGetPayloadAsync(string token)
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
        string? shopId = validationResult.ClaimsIdentity.FindFirst("shop_id")?.Value;
        string? stampsCount = validationResult.ClaimsIdentity.FindFirst("stamps_count")?.Value;

        if (string.IsNullOrWhiteSpace(tokenId)
            || !Guid.TryParse(stampId, out Guid parsedStampId)
            || !Guid.TryParse(shopId, out Guid parsedShopId)
            || !int.TryParse(stampsCount, out int parsedStampsCount)
            || parsedStampsCount <= 0)
        {
            return null;
        }

        DateTime expiresAtUtc = validationResult.SecurityToken.ValidTo;

        return new StampCollectionQrTokenPayload(
            tokenId,
            parsedStampId,
            parsedShopId,
            parsedStampsCount,
            expiresAtUtc);
    }
}
