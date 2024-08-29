using Api.Messaging;
using Api.Outbox.Models;
using Api.Outbox.Publications;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Polly;
using Quartz;

namespace Api.Outbox.Jobs;

public sealed class ProcessOutboxMessagesJob<TDbContext>(
    TDbContext dbContext,
    IPublisher publisher,
    TimeProvider timeProvider)
    : IJob
    where TDbContext : DbContext
{
    private readonly TDbContext _dbContext = dbContext;

    private readonly IPublisher _publisher = publisher;

    private readonly TimeProvider _timeProvider = timeProvider;

    public async Task Execute(IJobExecutionContext context)
    {
        var outboxMessages = await GetUnprocessedOutboxMessagesAsync(context.CancellationToken);

        foreach (var outboxMessage in outboxMessages)
        {
            var domainEvent = DeserializeOutboxMessageToDomainEvent(outboxMessage);

            if (domainEvent is null)
            {
                outboxMessage.MarkFailed(
                    _timeProvider.GetUtcNow(),
                    $"Failed to deserialize outbox message with ID: {outboxMessage.Id}.");

                outboxMessage.Quarantine.Activate(_timeProvider.GetUtcNow());

                continue;
            }

            var result = await PublishDomainEventAsync(domainEvent, context.CancellationToken);

            if (result.Outcome == OutcomeType.Successful)
            {
                outboxMessage.MarkSuccessful(_timeProvider.GetUtcNow());
            }
        }

        await _dbContext.SaveChangesAsync(context.CancellationToken);
    }

    private async Task<List<OutboxMessage>> GetUnprocessedOutboxMessagesAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext
            .Set<OutboxMessage>()
            .Where(m => !m.Quarantine.IsActive())
            .Where(m => !m.IsProcessed)
            .OrderBy(m => m.OccurredOnUtc)
            .Take(20)
            .ToListAsync(cancellationToken);
    }

    private static DomainEvent? DeserializeOutboxMessageToDomainEvent(
        OutboxMessage outboxMessage)
    {
        return JsonConvert
            .DeserializeObject<DomainEvent>(
                outboxMessage.Payload,
                new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All
                });
    }

    private async Task<PolicyResult> PublishDomainEventAsync(
        DomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        const int retryCount = 3;

        const int sleepDurationInMilliseconds = 100;

        var policy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount,
                _ => TimeSpan.FromMilliseconds(sleepDurationInMilliseconds),
                onRetryAsync: async (ex, _, _) => await PersistFailedAttempt(ex));

        return await policy.ExecuteAndCaptureAsync(() =>
            _publisher.Publish(
                domainEvent,
                cancellationToken));
    }

    private async Task PersistFailedAttempt(Exception exception)
    {
        await _dbContext
            .Set<OutboxMessageAttempt>()
            .AddAsync(new OutboxMessageAttempt(
                _timeProvider.GetUtcNow(),
                new FailedPublication(exception.Message)));
    }
}
