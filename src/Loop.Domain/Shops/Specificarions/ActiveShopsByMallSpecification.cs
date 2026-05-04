using Loop.Domain.Specifications;
using Loop.Domain.Shops;

namespace Loop.Domain.Shops.Specificarions;

public class ActiveShopsByMallSpecification : Specification<Shop>
{
    public ActiveShopsByMallSpecification(Guid mallId)
        : base(s => s.MallId == mallId && s.IsActive)
    {
    }
}
