using System;
using System.Collections.Generic;
using System.Text;
using Loop.Domain.Shops;
using Loop.Domain.Specifications;

namespace Loop.Domain.Shops.Specificarions;

public class ShopByMallSpecification:Specification<Shop>
{
    public ShopByMallSpecification(Guid mallId) :base(s => s.MallId == mallId)
    {
    }
}
