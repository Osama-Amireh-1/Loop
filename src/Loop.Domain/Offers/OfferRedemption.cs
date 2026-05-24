using Loop.Domain.Receipts;
using Loop.Domain.Shops;
using Loop.Domain.Users;
using Loop.SharedKernel;

namespace Loop.Domain.Offers;

public class OfferRedemption : AggregateRoot
{
    public Guid RedemptionId { get; private set; }
    public Guid OfferId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid ShopId { get; private set; }
    public Guid? QrId { get; private set; }
    public OfferRedemptionStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ConfirmedAt { get; private set; }

    private OfferRedemption() { }

    public static OfferRedemption Create(
        Guid offerId,
        Guid userId,
        Guid shopId,
        Guid? qrId = null)
        => new()
        {
            RedemptionId = Guid.NewGuid(),
            OfferId = offerId,
            UserId = userId,
            ShopId = shopId,
            QrId = qrId,
            Status = OfferRedemptionStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

    public void Confirm()
    {
        if (Status != OfferRedemptionStatus.Pending)
            throw new DomainException("Only pending redemptions can be confirmed.");

        Status = OfferRedemptionStatus.Confirmed;
        ConfirmedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == OfferRedemptionStatus.Confirmed)
            throw new DomainException("Cannot cancel a confirmed redemption.");

        Status = OfferRedemptionStatus.Cancelled;
    }
}


