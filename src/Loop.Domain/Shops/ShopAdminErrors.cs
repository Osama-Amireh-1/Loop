using Loop.SharedKernel;

namespace Loop.Domain.Shops;

public static class ShopAdminErrors
{
    public static Error NotFound(Guid shopAdminId) => Error.NotFound(
        "Shops.AdminNotFound",
        $"The shop admin with the Id = '{shopAdminId}' was not found");
}


