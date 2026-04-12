using Loop.Domain.Receipts;
using Loop.Domain.Shops;
using Loop.Domain.Users;
using Loop.SharedKernel;

namespace Loop.Domain.Stamps;

public class StampTransaction : AggregateRoot
{
    public Guid StampTxId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid ShopId { get; private set; }
    public Guid StampProgramId { get; private set; }
    public StampType Type { get; private set; }
    public int StampsCount { get; private set; }
    public Guid? RedemptionRef { get; private set; }    
    public DateTime CreatedAt { get; private set; }
    public User User { get; private set; }
    public Shop Shop { get; private set; }

    private StampTransaction() { }

    public static StampTransaction RecordCollect(
        Guid userId,
        Guid shopId,
        Guid stampProgramId,
        int count,
        Guid? receiptId)
        => new()
        {
            StampTxId = Guid.NewGuid(),
            UserId = userId,
            ShopId = shopId,
            StampProgramId = stampProgramId,
            Type = StampType.Collect,
            StampsCount = count,
            RedemptionRef = receiptId,
            CreatedAt = DateTime.UtcNow
        };

    public static StampTransaction RecordReward(
        Guid userId,
        Guid shopId,
        Guid stampProgramId)
        => new()
        {
            StampTxId = Guid.NewGuid(),
            UserId = userId,
            ShopId = shopId,
            StampProgramId = stampProgramId,
            Type = StampType.Reward,
            StampsCount = 0,
            CreatedAt = DateTime.UtcNow
        };
}


