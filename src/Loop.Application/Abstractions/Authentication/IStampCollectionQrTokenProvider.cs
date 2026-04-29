namespace Loop.Application.Abstractions.Authentication;

public interface IStampCollectionQrTokenProvider
{
    string CreateToken(StampCollectionQrTokenPayload payload);
    Task<StampCollectionQrTokenPayload?> ValidateAndGetPayloadAsync(string token);
}

public sealed record StampCollectionQrTokenPayload(
    string TokenId,
    Guid StampId,
    Guid ShopId,
    int StampsCount,
    DateTime ExpiresAtUtc);
