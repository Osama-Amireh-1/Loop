using Loop.SharedKernel;

namespace Loop.Domain.Shops;

public class ShopPointsWallet : Entity
{
    public Guid ShopId { get; private set; }
    public int PointsReceived { get; private set; }
    public DateTime LastUpdated { get; private set; }

    public Shop Shop { get; private set; } = null!;

    private ShopPointsWallet() { }

    public static ShopPointsWallet Create(Guid shopId) => new()
    {
        ShopId = shopId,
        PointsReceived = 0,
        LastUpdated = DateTime.UtcNow
    };

    public void AddPoints(int points)
    {
        if (points <= 0)
            throw new DomainException("Points must be positive.");

        PointsReceived += points;
        LastUpdated = DateTime.UtcNow;
    }
}
