using System;
using System.Collections.Generic;
using System.Text;
using Loop.Domain.Shops;
using Loop.Domain.Specifications;

namespace Loop.Infrastructure.Shops.Specification;

public class ShopByMallSpecification:Specification<Shop>
{
    public ShopByMallSpecification(Guid mallId) :base(s => s.MallId == mallId)
    {
    }
}
