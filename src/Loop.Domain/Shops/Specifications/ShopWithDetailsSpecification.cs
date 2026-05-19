using System;
using System.Collections.Generic;
using System.Text;
using Loop.Domain.Specifications;

namespace Loop.Domain.Shops.Specifications;

public class ShopWithDetailsSpecification:Specification<Shop>
{
    public ShopWithDetailsSpecification(Guid shopId)
        : base(s => s.ShopId == shopId)
    {
        AddInclude(x => x.PointsWallet);
    }
}
