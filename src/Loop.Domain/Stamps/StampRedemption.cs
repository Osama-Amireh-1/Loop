using System;
using Loop.SharedKernel;

namespace Loop.Domain.Stamps;

public class StampRedemption : AggregateRoot
{
    public Guid RedemptionId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid ShopId { get; private set; }
    public Guid StampId { get; private set; }
    public Guid? RedemptionRef { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private StampRedemption() { }

    public static StampRedemption Create(
        Guid userId,
        Guid shopId,
        Guid stampId,
        Guid? redemptionRef)
        => new()
        {
            RedemptionId = Guid.NewGuid(),
            UserId = userId,
            StampId = stampId,
            ShopId = shopId,
            RedemptionRef = redemptionRef,
            CreatedAt = DateTime.UtcNow
        };
}
