using BuildingBlocks.CleanArchitecture.Domain.Core;
using BuildingBlocks.CleanArchitecture.Infrastructure.Persistence.Database.InboxOutbox;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Text.Json;

namespace BuildingBlocks.CleanArchitecture.Infrastructure.Persistence.Database.Interceptors;

public sealed class ConvertDomainEventsToOutboxMessagesInterceptor
    : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var dbContext = eventData.Context;

        if (dbContext is null)
        {
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        if (dbContext is MessagingDbContextBase messagingDbContext)
        {
            var outboxMessages = messagingDbContext.ChangeTracker
                .Entries<IAggregateRoot>()
                .Select(x => x.Entity)
                .SelectMany(aggregateRoot =>
                {
                    var domainEvents = aggregateRoot.DomainEvents;
                    aggregateRoot.ClearDomainEvents();
                    return domainEvents;
                })
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
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
