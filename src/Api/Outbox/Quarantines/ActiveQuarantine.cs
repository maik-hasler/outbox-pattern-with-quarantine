namespace Api.Outbox.Quarantines;

public sealed record ActiveQuarantine(
    DateTimeOffset EnteredOnUtc)
    : IQuarantine;
