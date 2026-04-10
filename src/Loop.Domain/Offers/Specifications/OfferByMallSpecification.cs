using System;
using System.Collections.Generic;
using System.Text;
using Loop.Domain.Offers;
using Loop.Domain.Specifications;

namespace Loop.Domain.Offers.Specifications;

public class OfferByMallSpecification:Specification<Offer>
{
    public OfferByMallSpecification(Guid mallId) :base(o=>o.Shop.MallId== mallId)
    {
        AddInclude(o => o.Shop);
        AddInclude(o => o.Redemptions);

    }
}

