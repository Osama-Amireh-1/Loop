using Loop.SharedKernel;

namespace Loop.Domain.Shops;

public static class ShopErrors
{
    public static Error NotFound(Guid shopId) => Error.NotFound(
        "Shops.NotFound",
        $"The shop with the name = '{shopId}' was not found");

    public static readonly Error NameNotUnique = Error.Conflict(
        "Shops.NameNotUnique",
        "A shop with the specified name already exists");

    public static readonly Error ShopInactive = Error.Failure(
        "Shops.ShopInactive",
        "The shop is currently inactive");

    public static Error CategoryNotFound(Guid categoryId) => Error.NotFound(
        "Shops.CategoryNotFound",
        $"The category with the Id = '{categoryId}' was not found");

    public static readonly Error CategoryNameNotUnique = Error.Conflict(
        "Shops.CategoryNameNotUnique",
        "A category with the specified name already exists");

    public static Error AdminNotFound(Guid shopAdminId) => Error.NotFound(
        "Shops.AdminNotFound",
        $"The shop admin with the Id = '{shopAdminId}' was not found");

    public static readonly Error AdminEmailNotUnique = Error.Conflict(
        "Shops.AdminEmailNotUnique",
        "A shop admin with the specified email already exists");

    public static readonly Error AdminPhoneNotUnique = Error.Conflict(
        "Shops.AdminPhoneNotUnique",
        "A shop admin with the specified phone already exists");

    public static readonly Error QrCodeExpired = Error.Failure(
        "Shops.QrCodeExpired",
        "The QR code has expired");
}


