using SharedKernel;

namespace Domain.Transactions;

public sealed record PointsEarnedDomainEvent(
    Guid EarnId,
    Guid UserId,
    Guid ShopId,
    int PointsEarned) : IDomainEvent;
