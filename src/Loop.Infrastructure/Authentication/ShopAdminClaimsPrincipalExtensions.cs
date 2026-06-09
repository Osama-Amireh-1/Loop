using System.Security.Claims;
using Loop.Domain.Shops;

namespace Loop.Infrastructure.Authentication;

internal static class ShopAdminClaimsPrincipalExtensions
{
    public static Guid GetShopAdminId(this ClaimsPrincipal? principal)
    {
        string? shopAdminId = principal?.FindFirstValue("shop_admin_id");
        return Guid.TryParse(shopAdminId, out Guid parsedShopAdminId)
            ? parsedShopAdminId
            : throw new ApplicationException("Shop admin id is unavailable");
    }

    public static Guid GetShopId(this ClaimsPrincipal? principal)
    {
        string? shopId = principal?.FindFirstValue("shop_id");
        return Guid.TryParse(shopId, out Guid parsedShopId)
            ? parsedShopId
            : throw new ApplicationException("Shop id is unavailable");
    }

    public static ShopAdminRole GetShopAdminRole(this ClaimsPrincipal? principal)
    {
        string? role = principal?.FindFirstValue("shop_admin_role");
        return Enum.TryParse<ShopAdminRole>(role, out var parsedRole)
            ? parsedRole
            : throw new ApplicationException("Shop admin role is unavailable");
    }
}
