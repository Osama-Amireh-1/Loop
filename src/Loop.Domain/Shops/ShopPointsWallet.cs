using Loop.SharedKernel;

namespace Loop.Domain.Shops;

public class ShopPointsWallet : Entity
{
    public Guid ShopId { get; private set; }
    public int PointsReceived { get; private set; }
    public DateTime LastUpdated { get; private set; }

    public Shop Shop { get; private set; } = null!;

    private static readonly Error InvalidPointsError = new(
        "Shops.InvalidPoints",
        "Points must be positive.",
        ErrorType.Validation);

    private ShopPointsWallet() { }

    public static ShopPointsWallet Create(Guid shopId) => new()
    {
        ShopId = shopId,
        PointsReceived = 0,
        LastUpdated = DateTime.UtcNow
    };

    public Result AddPoints(int points)
    {
        if (points <= 0)
        {
            return Result.Failure(InvalidPointsError);
        }

        PointsReceived += points;
        LastUpdated = DateTime.UtcNow;
        return Result.Success();
    }
}
