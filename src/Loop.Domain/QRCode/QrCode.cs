using Domain.Shops;
using Domain.Users;
using Loop.SharedKernel;

namespace Domain.QRCode;

public sealed class QrCode : AggregateRoot
{
    private QrCode() { }

    private QrCode(Guid qrId, Guid? userId, Guid? shopId, string qrCodeData, DateTime expiresAt)
    {
        QrId = qrId;
        UserId = userId;
        ShopId = shopId;
        QrCodeData = qrCodeData;
        ExpiresAt = expiresAt;
    }

    public Guid QrId { get; private set; }
    public Guid? UserId { get; private set; }
    public Guid? ShopId { get; private set; }
    public string QrCodeData { get; private set; } = null!;
    public DateTime ExpiresAt { get; private set; }
    public User? User { get; private set; }
    public Shop? Shop { get; private set; }

    public static QrCode Create(
        Guid? userId,
        Guid? shopId,
        string qrCodeData,
        DateTime expiresAt)
    {
        return new QrCode(Guid.NewGuid(), userId, shopId, qrCodeData, expiresAt);
    }

    public bool IsExpired(DateTime utcNow) => utcNow >= ExpiresAt;
}
