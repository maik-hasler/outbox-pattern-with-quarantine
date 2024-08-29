using MediatR;

namespace Api.Messaging;

public interface IDomainEventHandler<in TDomainEvent>
    : INotificationHandler<TDomainEvent>
    where TDomainEvent : DomainEvent;