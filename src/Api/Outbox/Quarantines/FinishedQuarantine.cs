namespace Api.Outbox.Quarantines;

public sealed record FinishedQuarantine(
    DateTimeOffset EnteredOnUtc,
    DateTimeOffset ReleasedOnUtc)
    : IQuarantine;
