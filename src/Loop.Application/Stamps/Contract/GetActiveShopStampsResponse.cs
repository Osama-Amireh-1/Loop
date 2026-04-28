namespace Loop.Application.Stamps.Contract;

public sealed class GetActiveShopStampsResponse
{
    public required Guid StampId { get; init; }
    public required string StampName { get; init; }
    public required string StampDescription { get; init; }
    public string? ImageUrl { get; init; }
    public string? IconUrl { get; init; }
    public required int StampsRequired { get; init; }
    public required string RewardType { get; init; }
    public required DateTime StartDate { get; init; }
    public required DateTime EndDate { get; init; }
}
