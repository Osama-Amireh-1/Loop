using Loop.Domain.Specifications;
using Loop.Domain.Stamps;

namespace Loop.Domain.Stamps.Specifications;

public class ActiveStampsByShopSpecification : Specification<Stamp>
{
    public ActiveStampsByShopSpecification(Guid shopId)
        : base(s => s.ShopId == shopId
            && s.IsActive
            && s.StartDate <= DateTime.UtcNow
            && s.EndDate >= DateTime.UtcNow)
    {
        AddInclude(s => s.Shop);
    }
}
