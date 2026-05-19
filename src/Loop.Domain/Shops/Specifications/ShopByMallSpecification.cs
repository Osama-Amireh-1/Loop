using System;
using System.Collections.Generic;
using System.Text;
using Loop.Domain.Specifications;

namespace Loop.Domain.Shops.Specifications;

public class ShopByMallSpecification:Specification<Shop>
{
    public ShopByMallSpecification(Guid mallId) :base(s => s.MallId == mallId)
    {
    }
}
