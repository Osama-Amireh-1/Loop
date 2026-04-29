namespace Loop.Application.Stamps.Contract;

public sealed class GenerateStampCollectQrResponse
{
    public required Guid QrId { get; init; }
    public required DateTime ExpiresAtUtc { get; init; }
}
