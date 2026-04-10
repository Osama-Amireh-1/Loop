using Loop.SharedKernel;

namespace Loop.Domain.Users;

public sealed record UserRegisteredDomainEvent(Guid UserId) : IDomainEvent;


