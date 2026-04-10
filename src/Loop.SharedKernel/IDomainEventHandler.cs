using MediatR;

namespace Loop.SharedKernel;

public interface IDomainEventHandler<in T> : INotificationHandler<T>
    where T : IDomainEvent;

