using Loop.Domain.Common;
using Loop.Domain.Specifications;

namespace Loop.Domain.Shops.Specifications;

public sealed class ShopAdminByEmailSpecification : Specification<ShopAdmin>
{
    public ShopAdminByEmailSpecification(Loop.Domain.Common.Email email)
        : base(admin => admin.Email == email)
    {
    }
}
