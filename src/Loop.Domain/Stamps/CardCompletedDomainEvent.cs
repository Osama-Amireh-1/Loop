using Loop.SharedKernel;

namespace Loop.Domain.Stamps;

public sealed record CardCompletedDomainEvent(Guid UserId, Guid StampId) : IDomainEvent;


