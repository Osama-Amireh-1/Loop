using System;
using Loop.SharedKernel;

namespace Loop.Domain.Stamps;

public class StampRedemption : AggregateRoot
{
    public Guid RedemptionId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid ShopId { get; private set; }
    public Guid StampId { get; private set; }
    public Guid? QrId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private StampRedemption() { }

    public static StampRedemption Create(
        Guid userId,
        Guid shopId,
        Guid stampId,
        Guid? QrId)
        => new()
        {
            RedemptionId = Guid.NewGuid(),
            UserId = userId,
            StampId = stampId,
            ShopId = shopId,
            QrId = QrId,
            CreatedAt = DateTime.UtcNow
        };
}
