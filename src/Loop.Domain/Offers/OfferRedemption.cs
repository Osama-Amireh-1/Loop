using Loop.Domain.Receipts;
using Loop.Domain.Shops;
using Loop.Domain.Users;
using Loop.SharedKernel;

namespace Loop.Domain.Offers;

public class OfferRedemption : Entity
{
    public Guid RedemptionId { get; private set; }
    public Guid OfferId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid ShopId { get; private set; }
    public Guid? RedemptionRef { get; private set; }  
    public DateTime CreatedAt { get; private set; }

    private OfferRedemption() { }

    internal static OfferRedemption Create(
        Guid offerId,
        Guid userId,
        Guid shopId,
        Guid? receiptId)
        => new()
        {
            RedemptionId = Guid.NewGuid(),
            OfferId = offerId,
            UserId = userId,
            ShopId = shopId,
            RedemptionRef = receiptId,
            CreatedAt = DateTime.UtcNow
        };
}


