using Loop.SharedKernel;

namespace Loop.Domain.Shops;

public class ShopAdminSession : AggregateRoot
{
    public Guid SessionId { get; private set; }
    public Guid ShopAdminId { get; private set; }
    public string RefreshTokenHash { get; private set; }
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private ShopAdminSession() { }

    public static ShopAdminSession Create(Guid shopAdminId, string refreshTokenHash, DateTime expiresAtUtc)
    {
        if (shopAdminId == Guid.Empty)
            throw new DomainException("ShopAdminId is required.");

        if (string.IsNullOrWhiteSpace(refreshTokenHash))
            throw new DomainException("Refresh token hash is required.");

        if (expiresAtUtc <= DateTime.UtcNow)
            throw new DomainException("Session expiry must be in the future.");

        return new ShopAdminSession
        {
            SessionId = Guid.NewGuid(),
            ShopAdminId = shopAdminId,
            RefreshTokenHash = refreshTokenHash,
            ExpiresAtUtc = expiresAtUtc,
            CreatedAt = DateTime.UtcNow
        };
    }

    public bool IsExpired() => ExpiresAtUtc <= DateTime.UtcNow;
}
