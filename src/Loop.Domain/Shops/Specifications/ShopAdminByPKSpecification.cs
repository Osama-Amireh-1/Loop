using Loop.Domain.Specifications;

namespace Loop.Domain.Shops.Specifications;

public sealed class ShopAdminByPKSpecification : Specification<ShopAdmin>
{
    public ShopAdminByPKSpecification(Guid id)
        : base(admin => admin.ShopAdminId == id)
    {
    }
}
