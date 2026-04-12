using System;
using System.Collections.Generic;
using System.Text;
using Loop.Domain.Specifications;

namespace Loop.Domain.Shops.Specificarions;

public class ShopByPKSpecification:Specification<Shop>
{

    public ShopByPKSpecification(Guid mallId, Guid shopId):base(s => s.MallId == mallId && s.ShopId == shopId)
    {
        AddInclude(s=>s.Category);
    }
}
