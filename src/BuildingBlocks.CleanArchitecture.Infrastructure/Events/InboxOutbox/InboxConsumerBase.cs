using BuildingBlocks.CleanArchitecture.Application.Events;
using BuildingBlocks.CleanArchitecture.Infrastructure.Persistence.Database.InboxOutbox;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace BuildingBlocks.CleanArchitecture.Infrastructure.Events.InboxOutbox;

public abstract class InboxConsumerBase<TEvent>
    : IConsumer<TEvent>
    where TEvent : class, IIntegrationEvent
{
    private readonly DbContext _dbContext;
    private readonly ILogger _logger;

    protected InboxConsumerBase(DbContext dbContext, ILogger logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<TEvent> context)
    {
        if (_dbContext is not MessagingDbContextBase messagingDbContext)
        {
            return;
        }

        var messageId = context.MessageId ?? context.Message.Id;

        var alreadyProcessed = await messagingDbContext
            .Set<InboxMessage>()
            .AnyAsync(x => x.MessageId == messageId, context.CancellationToken);

        if (alreadyProcessed)
        {
            _logger.LogInformation("Duplicate message {MessageId} skipped", messageId);
            return;
        }

        var inboxMessage = new InboxMessage
        {
            Id = Guid.NewGuid(),
            MessageId = messageId,
            Type = typeof(TEvent).AssemblyQualifiedName!,
            Content = JsonSerializer.Serialize(context.Message),
            ReceivedOnUtc = DateTime.UtcNow
        };

        await messagingDbContext.Set<InboxMessage>().AddAsync(inboxMessage).ConfigureAwait(false);
        await messagingDbContext.SaveChangesAsync(context.CancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Stored message {MessageId} in inbox", messageId);
    }
}
