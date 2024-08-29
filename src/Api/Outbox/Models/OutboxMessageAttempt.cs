using Api.Outbox.Publications;

namespace Api.Outbox.Models;

public sealed class OutboxMessageAttempt(
    DateTimeOffset attemptedOnUtc,
    IPublicationResult publicationResult)
{
    public OutboxMessage OutboxMessage { get; set; }

    public DateTimeOffset AttemptedOnUtc { get; private init; } = attemptedOnUtc;

    public IPublicationResult PublicationResult { get; private init; } = publicationResult;
}
