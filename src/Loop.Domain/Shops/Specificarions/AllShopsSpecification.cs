using Loop.Domain.Specifications;
using Loop.Domain.Shops;

namespace Loop.Domain.Shops.Specificarions;

public class AllShopsSpecification : Specification<Shop>
{
    public AllShopsSpecification() : base(s => true)
    {
    }
}
