using System;
using System.Collections.Generic;
using System.Text;
using Loop.Domain.Offers;
using Loop.Domain.Specifications;

namespace Loop.Domain.Offers.Specifications;

public class ActiveOfferByMallSpecification:Specification<Offer>
{
    public ActiveOfferByMallSpecification(Guid mallId) :base(o=>o.IsActive&& o.StartDate<= DateTime.Now&& o.EndDate >= DateTime.Now && o.Shop.MallId== mallId)
    {
        AddInclude(o => o.Shop);
        AddInclude(o => o.Redemptions);

    }
}

