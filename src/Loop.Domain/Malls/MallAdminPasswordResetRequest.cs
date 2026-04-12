using Loop.SharedKernel;

namespace Loop.Domain.Malls;

public class MallAdminPasswordResetRequest : AggregateRoot
{
    public Guid RequestId { get; private set; }
    public Guid MallAdminId { get; private set; }
    public string TokenHash { get; private set; }
    public DateTime ExpiresAtUtc { get; private set; }

    private MallAdminPasswordResetRequest() { }

    public static MallAdminPasswordResetRequest Create(Guid mallAdminId, string tokenHash, DateTime expiresAtUtc)
    {
        if (mallAdminId == Guid.Empty)
            throw new DomainException("MallAdminId is required.");

        if (string.IsNullOrWhiteSpace(tokenHash))
            throw new DomainException("Token hash is required.");

        if (expiresAtUtc <= DateTime.UtcNow)
            throw new DomainException("Reset token expiry must be in the future.");

        return new MallAdminPasswordResetRequest
        {
            RequestId = Guid.NewGuid(),
            MallAdminId = mallAdminId,
            TokenHash = tokenHash,
            ExpiresAtUtc = expiresAtUtc
        };
    }

    public bool IsExpired() => ExpiresAtUtc <= DateTime.UtcNow;
}
