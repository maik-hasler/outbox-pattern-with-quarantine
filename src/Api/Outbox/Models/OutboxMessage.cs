using Api.Outbox.Publications;
using Api.Outbox.Quarantines;

namespace Api.Outbox.Models;

public sealed class OutboxMessage(
    Guid id,
    string topic,
    string payload,
    DateTimeOffset occurredOnUtc)
{
    public Guid Id { get; private init; } = id;

    public string Topic { get; private init; } = topic;

    public string Payload { get; private init; } = payload;

    public DateTimeOffset OccurredOnUtc { get; private init; } = occurredOnUtc;

    public Quarantine Quarantine { get; private init; } = Quarantine.Inactive;

    private readonly List<OutboxMessageAttempt> _attempts = [];

    public IReadOnlyCollection<OutboxMessageAttempt> Attempts => _attempts;

    private readonly List<OutboxMessageQuarantine> _quarantines = [];

    public IReadOnlyCollection<OutboxMessageQuarantine> Quarantines => _quarantines;

    public bool IsProcessed => _attempts.Exists(a => a.PublicationResult is SuccessfulPublication);

    public void MarkSuccessful(DateTimeOffset attemptedOnUtc)
    {
        _attempts.Add(new OutboxMessageAttempt(
            attemptedOnUtc,
            new SuccessfulPublication()));
    }

    public void MarkFailed(DateTimeOffset attemptedOnUtc, string error)
    {
        _attempts.Add(new OutboxMessageAttempt(
            attemptedOnUtc,
            new FailedPublication(error)));
    }

    public void ActivateQuarantine(DateTimeOffset enteredOnUtc)
    {
        _quarantines.Add(new OutboxMessageQuarantine(this, enteredOnUtc));
    }
}
