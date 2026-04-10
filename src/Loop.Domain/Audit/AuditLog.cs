using Loop.Domain.Malls;
using Loop.Domain.Shops;
using Loop.Domain.Users;
using Loop.SharedKernel;

namespace Loop.Domain.Audit;

public class AuditLog : AggregateRoot
{
    public Guid LogId { get; private set; }
    public Guid? UserId { get; private set; }
    public Guid? ShopId { get; private set; }
    public Guid? ShopAdminId { get; private set; }
    public Guid? MallAdminId { get; private set; }
    public AdminType? AdminType { get; private set; }
    public string ActionType { get; private set; }
    public int? Points { get; private set; }
    public string? Metadata { get; private set; }   // JSON string
    public DateTime CreatedAt { get; private set; }

    public User? User { get; private set; }
    public Shop? Shop { get; private set; }

    public ShopAdmin? ShopAdmin { get; private set; }

    public MallAdmin? MallAdmin { get; private set; }

    private AuditLog() { }

    public static AuditLog Record(
        string actionType,
        Guid? userId = null,
        Guid? shopId = null,
         Guid? mallAdminId = null,
        Guid? shopAdminId = null,
        AdminType? adminType = null,
        int? points = null,
        string? metadata = null)
        => new()
        {
            LogId = Guid.NewGuid(),
            ActionType = actionType,
            UserId = userId,
            ShopId = shopId,
            MallAdminId = mallAdminId,
            ShopAdminId=shopAdminId,
            AdminType = adminType,
            Points = points,
            Metadata = metadata,
            CreatedAt = DateTime.UtcNow
        };
}


