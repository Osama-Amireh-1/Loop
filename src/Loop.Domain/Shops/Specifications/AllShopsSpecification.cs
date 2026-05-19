using Loop.Domain.Specifications;

namespace Loop.Domain.Shops.Specifications;

public class AllShopsSpecification : Specification<Shop>
{
    public AllShopsSpecification() : base(s => true)
    {
    }
}
