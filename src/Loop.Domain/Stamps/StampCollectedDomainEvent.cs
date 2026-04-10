using Loop.SharedKernel;

namespace Loop.Domain.Stamps;

public sealed record StampCollectedDomainEvent(Guid UserId, Guid StampId) : IDomainEvent;


