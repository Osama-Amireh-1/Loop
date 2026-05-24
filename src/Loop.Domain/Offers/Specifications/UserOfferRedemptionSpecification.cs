using Loop.Domain.Specifications;

namespace Loop.Domain.Offers.Specifications;

public class UserOfferRedemptionSpecification : Specification<OfferRedemption>
{
    public UserOfferRedemptionSpecification(Guid userId, Guid offerId)
        : base(r => r.UserId == userId && 
                    r.OfferId == offerId && 
                    r.Status == OfferRedemptionStatus.Confirmed)
    {
    }
}
