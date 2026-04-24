using System.Security.Claims;

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
}
