using Loop.Domain.Specifications;

namespace Loop.Domain.Shops.Specifications;

public class ShopByIdSpecification : Specification<Shop>
{
    public ShopByIdSpecification(Guid shopId)
        : base(s => s.ShopId == shopId)
    {
    }
}
