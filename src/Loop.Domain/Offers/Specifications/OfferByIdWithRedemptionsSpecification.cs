using Loop.Domain.Specifications;

namespace Loop.Domain.Offers.Specifications;

public class OfferByIdWithRedemptionsSpecification : Specification<Offer>
{
    public OfferByIdWithRedemptionsSpecification(Guid offerId) 
        : base(o => o.OfferId == offerId && o.IsActive && 
                    o.StartDate <= DateTime.UtcNow && 
                    o.EndDate >= DateTime.UtcNow)
    {
        AddInclude(o => o.Shop);
        AddInclude(o => o.Redemptions);
    }
}
