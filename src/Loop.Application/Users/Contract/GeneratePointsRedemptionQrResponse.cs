namespace Loop.Application.Users.Contract;

public sealed class GeneratePointsRedemptionQrResponse
{
    public required Guid QrId { get; init; }
    public required string QrCodeData { get; init; }
    public required DateTime ExpiresAtUtc { get; init; }
}
