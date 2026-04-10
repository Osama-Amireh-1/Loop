using System.Globalization;
using System.Security.Cryptography;
using Loop.Domain.Common;
using Loop.Domain.Shops;
using Loop.Domain.Users;
using Loop.SharedKernel;

namespace Loop.Domain.Transactions;

public class RedeemTransaction : AggregateRoot
{
    public Guid RedeemId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid ShopId { get; private set; }
    public int PointsUsed { get; private set; }
    public Money DiscountValue { get; private set; }
    public string VerificationCode { get; private set; }
    public RedemptionStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    public User User { get; private set; }
    public Shop Shop { get; private set; }

    private RedeemTransaction() { }

    public static RedeemTransaction Initiate(
        Guid userId,
        Guid shopId,
        int pointsUsed,
        Money discountValue)
        => new()
        {
            RedeemId = Guid.NewGuid(),
            UserId = userId,
            ShopId = shopId,
            PointsUsed = pointsUsed,
            DiscountValue = discountValue,
            VerificationCode = RandomNumberGenerator.GetInt32(100000, 999999).ToString(CultureInfo.InvariantCulture),
            Status = RedemptionStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

    public void Verify()
    {
        if (Status != RedemptionStatus.Pending)
            throw new DomainException("Only pending redemptions can be verified.");
        Status = RedemptionStatus.Verified;
        CompletedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status != RedemptionStatus.Pending)
            throw new DomainException("Only pending redemptions can be cancelled.");
        Status = RedemptionStatus.Cancelled;
        CompletedAt = DateTime.UtcNow;
    }
}


