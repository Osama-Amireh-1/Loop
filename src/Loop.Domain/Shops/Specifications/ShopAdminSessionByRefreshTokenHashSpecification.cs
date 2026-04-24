using Loop.Domain.Specifications;

namespace Loop.Domain.Shops.Specifications;

public sealed class ShopAdminSessionByRefreshTokenHashSpecification : Specification<ShopAdminSession>
{
    public ShopAdminSessionByRefreshTokenHashSpecification(string refreshTokenHash)
        : base(session => session.RefreshTokenHash == refreshTokenHash)
    {
    }
}
