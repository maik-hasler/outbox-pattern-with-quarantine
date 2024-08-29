using MediatR;

namespace Api.Messaging;

public abstract record DomainEvent(
    Guid Id,
    DateTimeOffset OccurredOnUtc)
    : INotification;
