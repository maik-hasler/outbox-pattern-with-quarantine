namespace Api.Outbox.Publications;

public sealed record FailedPublication(string Error) : IPublicationResult;