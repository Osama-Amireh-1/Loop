using SharedKernel;

namespace Domain.Transactions;

public sealed record PointsRedeemedDomainEvent(
    Guid RedeemId,
    Guid UserId,
    Guid ShopId,
    int PointsUsed,
    decimal DiscountValue) : IDomainEvent;
