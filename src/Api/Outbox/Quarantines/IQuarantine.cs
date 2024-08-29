namespace Api.Outbox.Quarantines;

public interface IQuarantine
{
    DateTimeOffset EnteredOnUtc { get; }
}
