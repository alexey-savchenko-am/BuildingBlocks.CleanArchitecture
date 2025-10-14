using BuildingBlocks.CleanArchitecture.Application.Events;
using BuildingBlocks.CleanArchitecture.Infrastructure.Persistence.Database.InboxOutbox;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace BuildingBlocks.CleanArchitecture.Infrastructure.Events;

public class PersistentEventBus(DbContext DbContext): IEventBus
{
    JsonSerializerOptions _options = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) 
        where T : IIntegrationEvent
    {
        if (DbContext is not MessagingDbContextBase messagingDbContext)
        {
            return;
        }

        await messagingDbContext
            .Set<OutboxMessage>()
            .AddAsync(new OutboxMessage
            {
                Id = Guid.NewGuid(),
                OccuredOnUtc = DateTime.UtcNow,
                Type = typeof(T).AssemblyQualifiedName!,
                MessageType = MessageType.IntegrationEvent,
                Content = JsonSerializer.Serialize(message, _options)
            }, cancellationToken);

        // Don't perform SaveChanges here. 
    }
}
