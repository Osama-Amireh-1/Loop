namespace Loop.Application.Abstractions.Authentication;

public interface IPointsRedemptionQrTokenProvider
{
    string CreateToken(PointsRedemptionQrTokenPayload payload);
    Task<PointsRedemptionQrTokenPayload?> ValidateAndGetPayloadAsync(string token);
}

public sealed record PointsRedemptionQrTokenPayload(
    string TokenId,
    Guid UserId,
    int PointsToRedeem,
    DateTime ExpiresAtUtc);
