using Loop.Domain.Common;
using Loop.Domain.Specifications;
using Microsoft.EntityFrameworkCore;

namespace Loop.Domain.Shops.Specifications;

public sealed class ShopAdminByEmailSpecification : Specification<ShopAdmin>
{
    public ShopAdminByEmailSpecification(Loop.Domain.Common.Email email)
    : base(admin => EF.Functions.ILike(admin.Email.Value, email.Value))
    {
    }
}
