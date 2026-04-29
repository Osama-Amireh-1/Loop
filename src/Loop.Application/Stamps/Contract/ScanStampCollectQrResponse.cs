namespace Loop.Application.Stamps.Contract;

public sealed class ScanStampCollectQrResponse
{
    public required Guid StampId { get; init; }
    public required int StampsCounter { get; init; }
    public required bool IsCompleted { get; init; }
}
