namespace Loop.Application.Stamps.Contract;

public sealed class GenerateStampRedemptionQrResponse
{
    public required Guid QrId { get; init; }
    public required string QrCodeData { get; init; }
    public required DateTime ExpiresAtUtc { get; init; }
}
