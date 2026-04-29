using Loop.Domain.Specifications;

namespace Loop.Domain.Shops.Specificarions;

public class ShopByIdSpecification : Specification<Shop>
{
    public ShopByIdSpecification(Guid shopId)
        : base(s => s.ShopId == shopId)
    {
    }
}
