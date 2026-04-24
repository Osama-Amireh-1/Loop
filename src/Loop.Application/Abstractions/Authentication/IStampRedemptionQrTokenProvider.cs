namespace Loop.Application.Abstractions.Authentication;

public interface IStampRedemptionQrTokenProvider
{
    string CreateToken(StampRedemptionQrTokenPayload payload);
    Task<StampRedemptionQrTokenPayload?> ValidateAndGetPayloadAsync(string token);
}

public sealed record StampRedemptionQrTokenPayload(
    string TokenId,
    Guid StampId,
    Guid UserId,
    Guid ShopId,
    DateTime ExpiresAtUtc);
