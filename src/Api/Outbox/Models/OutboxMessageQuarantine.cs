namespace Api.Outbox.Models;

public sealed class OutboxMessageQuarantine(
    OutboxMessage outboxMessage,
    DateTimeOffset enteredOnUtc)
{
    public OutboxMessage OutboxMessage { get; private init; } = outboxMessage;

    public DateTimeOffset EnteredOnUtc { get; private init; } =  enteredOnUtc;

    public DateTimeOffset? ReleasedOnUtc { get; private set; }

    public void Release(DateTimeOffset releasedOnUtc) => ReleasedOnUtc = releasedOnUtc;
}