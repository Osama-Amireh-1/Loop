using Loop.SharedKernel;
using SharedKernel;

namespace Domain.Stamps;

public sealed record CardCompletedDomainEvent(Guid UserId, Guid StampId) : IDomainEvent;
