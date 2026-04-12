using System;
using System.Collections.Generic;
using System.Text;
using Loop.Domain.Specifications;

namespace Loop.Domain.Offers.Specifications;

public class ActiveOfferByPKSpecification:Specification<Offer>
{
    public ActiveOfferByPKSpecification(Guid mallId,  Guid shopId) : base(o => o.IsActive && o.StartDate <= DateTime.Now && o.EndDate >= DateTime.Now && o.Shop.MallId == mallId && o.ShopId==shopId)
    {
        AddInclude(o => o.Shop);
        AddInclude(o => o.Redemptions);

    }
}
