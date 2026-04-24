namespace Loop.Application.Abstractions.Authentication;

public interface IShopAdminContext
{
    Guid ShopAdminId { get; }
    Guid ShopId { get; }
}
