using Loop.Application.Abstractions.Authentication;
using Microsoft.AspNetCore.Http;

namespace Loop.Infrastructure.Authentication;

internal sealed class ShopAdminContext : IShopAdminContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ShopAdminContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid ShopAdminId => _httpContextAccessor.HttpContext?.User.GetShopAdminId() ?? throw new ShopAdminContextUnavailableException();

    public Guid ShopId => _httpContextAccessor.HttpContext?.User.GetShopId() ?? throw new ShopAdminContextUnavailableException();
}
