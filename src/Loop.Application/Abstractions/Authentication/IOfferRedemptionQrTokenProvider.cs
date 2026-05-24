namespace Loop.Application.Abstractions.Authentication;

public interface IOfferRedemptionQrTokenProvider
{
    string CreateToken(OfferRedemptionQrTokenPayload payload);
    Task<OfferRedemptionQrTokenPayload?> ValidateAndGetPayloadAsync(string token);
}

public sealed record OfferRedemptionQrTokenPayload(
    string TokenId,
    Guid OfferId,
    Guid UserId,
    DateTime ExpiresAtUtc);
