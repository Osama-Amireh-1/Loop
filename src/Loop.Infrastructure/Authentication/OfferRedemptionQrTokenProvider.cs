using System.Globalization;
using System.Security.Claims;
using System.Text;
using Loop.Application.Abstractions.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Loop.Infrastructure.Authentication;

internal sealed class OfferRedemptionQrTokenProvider(IConfiguration configuration) : IOfferRedemptionQrTokenProvider
{
    private static readonly JsonWebTokenHandler Handler = new();

    public string CreateToken(OfferRedemptionQrTokenPayload payload)
    {
        string secretKey = configuration["Jwt:Secret"]!;
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new Claim(JwtRegisteredClaimNames.Jti, payload.TokenId),
                new Claim("offer_id", payload.OfferId.ToString()),
                new Claim("user_id", payload.UserId.ToString())
            ]),
            Expires = payload.ExpiresAtUtc,
            SigningCredentials = credentials,
            Issuer = configuration["Jwt:Issuer"],
            Audience = configuration["Jwt:Audience"]
        };

        return Handler.CreateToken(tokenDescriptor);
    }

    public async Task<OfferRedemptionQrTokenPayload?> ValidateAndGetPayloadAsync(string token)
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
        string? offerId = validationResult.ClaimsIdentity.FindFirst("offer_id")?.Value;
        string? userId = validationResult.ClaimsIdentity.FindFirst("user_id")?.Value;

        if (string.IsNullOrWhiteSpace(tokenId)
            || !Guid.TryParse(offerId, out Guid parsedOfferId)
            || !Guid.TryParse(userId, out Guid parsedUserId))
        {
            return null;
        }

        DateTime expiresAtUtc = validationResult.SecurityToken.ValidTo;

        return new OfferRedemptionQrTokenPayload(
            tokenId,
            parsedOfferId,
            parsedUserId,
            expiresAtUtc);
    }
}
