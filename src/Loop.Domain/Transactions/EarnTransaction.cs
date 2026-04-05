using Domain.Common;
using Domain.Shops;
using Domain.Users;
using Loop.SharedKernel;

namespace Domain.Transactions;

public class EarnTransaction : AggregateRoot
{
    public Guid EarnId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid ShopId { get; private set; }
    public Money PurchaseAmount { get; private set; }
    public int PointsEarned { get; private set; }
    public string? TransactionRef { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public User User { get; private set; }

    public Shop Shop { get; private set; }

    private EarnTransaction() { }

    public static EarnTransaction Record(
        Guid userId,
        Guid shopId,
        Money purchaseAmount,
        int pointsEarned,
        string? transactionRef)
        => new()
        {
            EarnId = Guid.NewGuid(),
            UserId = userId,
            ShopId = shopId,
            PurchaseAmount = purchaseAmount,
            PointsEarned = pointsEarned,
            TransactionRef = transactionRef,
            CreatedAt = DateTime.UtcNow
        };
}
