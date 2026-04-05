using Loop.SharedKernel;
using SharedKernel;

namespace Domain.Stamps;

public sealed record StampCollectedDomainEvent(Guid UserId, Guid StampId) : IDomainEvent;
