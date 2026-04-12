using System;
using System.Collections.Generic;
using System.Text;
using Loop.Domain.Specifications;

namespace Loop.Domain.Stamps.Specificarions;

public class ActiveStampByPKSpecification:Specification<Stamp>
{
    public ActiveStampByPKSpecification(Guid mallId, Guid shopId):base(s=>s.Shop.MallId == mallId
    &&s.ShopId==shopId && s.IsActive && s.StartDate <= DateTime.UtcNow && s.EndDate>=DateTime.UtcNow)
    {
        AddInclude(s => s.Shop);

    }
}
