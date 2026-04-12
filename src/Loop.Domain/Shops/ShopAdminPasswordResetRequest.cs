using Loop.SharedKernel;

namespace Loop.Domain.Shops;

public class ShopAdminPasswordResetRequest : AggregateRoot
{
    public Guid RequestId { get; private set; }
    public Guid ShopAdminId { get; private set; }
    public string TokenHash { get; private set; }
    public DateTime ExpiresAtUtc { get; private set; }

    private ShopAdminPasswordResetRequest() { }

    public static ShopAdminPasswordResetRequest Create(Guid shopAdminId, string tokenHash, DateTime expiresAtUtc)
    {
        if (shopAdminId == Guid.Empty)
            throw new DomainException("ShopAdminId is required.");

        if (string.IsNullOrWhiteSpace(tokenHash))
            throw new DomainException("Token hash is required.");

        if (expiresAtUtc <= DateTime.UtcNow)
            throw new DomainException("Reset token expiry must be in the future.");

        return new ShopAdminPasswordResetRequest
        {
            RequestId = Guid.NewGuid(),
            ShopAdminId = shopAdminId,
            TokenHash = tokenHash,
            ExpiresAtUtc = expiresAtUtc
        };
    }

    public bool IsExpired() => ExpiresAtUtc <= DateTime.UtcNow;
}
