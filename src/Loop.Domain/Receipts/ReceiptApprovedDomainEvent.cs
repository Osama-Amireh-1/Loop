using SharedKernel;

namespace Domain.Receipts;

public sealed record ReceiptApprovedDomainEvent(
    Guid ReceiptId,
    Guid UserId,
    Guid ShopId,
    decimal Amount) : IDomainEvent;
