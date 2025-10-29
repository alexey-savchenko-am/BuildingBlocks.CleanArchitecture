using BuildingBlocks.CleanArchitecture.Domain.Core;
using BuildingBlocks.CleanArchitecture.Infrastructure.Persistence.Database.InboxOutbox;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace BuildingBlocks.CleanArchitecture.Infrastructure.Persistence.Database.Interceptors;

public sealed class ConvertDomainEventsToOutboxMessagesInterceptor
    : SaveChangesInterceptor
{
    private readonly ILogger<ConvertDomainEventsToOutboxMessagesInterceptor> _logger;

    public ConvertDomainEventsToOutboxMessagesInterceptor(
        ILogger<ConvertDomainEventsToOutboxMessagesInterceptor> logger)
    {
        _logger = logger;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var dbContext = eventData.Context;

        if (dbContext is null)
        {
            _logger.LogWarning("SavingChangesAsync called but DbContext is null.");
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        if (dbContext is not MessagingDbContextBase messagingDbContext)
        {
            _logger.LogDebug("DbContext {ContextType} does not inherit from MessagingDbContextBase. Skipping outbox conversion.", dbContext.GetType().Name);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        try
        {
            var aggregates = messagingDbContext.ChangeTracker
                .Entries<IAggregateRoot>()
                .Select(x => x.Entity)
                .ToList();

            if (aggregates.Count == 0)
            {
                _logger.LogTrace("No aggregate roots with domain events found during SaveChanges.");
                return base.SavingChangesAsync(eventData, result, cancellationToken);
            }

            var domainEvents = aggregates
                .SelectMany(aggregateRoot =>
                {
                    var events = aggregateRoot.DomainEvents.ToList();
                    aggregateRoot.ClearDomainEvents();
                    return events;
                })
                .ToList();

            if (domainEvents.Count == 0)
            {
                _logger.LogTrace("Aggregates found, but no domain events to convert.");
                return base.SavingChangesAsync(eventData, result, cancellationToken);
            }

            _logger.LogInformation(
                "Converting {Count} domain events to outbox messages for context {Context}.",
                domainEvents.Count,
                dbContext.GetType().Name);

            var outboxMessages = domainEvents
                .Select(domainEvent => new OutboxMessage
                {
                    Id = Guid.NewGuid(),
                    OccuredOnUtc = DateTime.UtcNow,
                    Type = domainEvent.GetType().AssemblyQualifiedName!,
                    MessageType = MessageType.DomainEvent,
                    Content = JsonSerializer.Serialize(domainEvent, domainEvent.GetType())
                })
                .ToList();

            messagingDbContext.Set<OutboxMessage>().AddRange(outboxMessages);

            _logger.LogDebug("Added {Count} outbox messages to DbContext.", outboxMessages.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while converting domain events to outbox messages.");
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
