using Loop.Domain.Specifications;

namespace Loop.Domain.Shops.Specifications;

public class ActiveShopsByMallSpecification : Specification<Shop>
{
    public ActiveShopsByMallSpecification(Guid mallId)
        : base(s => s.MallId == mallId && s.IsActive)
    {
    }
}
