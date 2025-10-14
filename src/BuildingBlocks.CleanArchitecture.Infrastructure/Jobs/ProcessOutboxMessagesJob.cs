using BuildingBlocks.CleanArchitecture.Infrastructure.Persistence.Database.InboxOutbox;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Quartz;
using System.Text.Json;

namespace BuildingBlocks.CleanArchitecture.Infrastructure.Jobs;

[DisallowConcurrentExecution]
public sealed class ProcessOutboxMessagesJob : IJob
{
    private readonly DbContext _dbContext;
    private readonly IPublisher _domainEventPublisher;
    private readonly IPublishEndpoint _integrationEventPublisher;

    public ProcessOutboxMessagesJob(
        DbContext dbContext,
        IPublisher domainEventPublisher,
        IPublishEndpoint integrationEventPublisher)
    {
        _dbContext = dbContext;
        _domainEventPublisher = domainEventPublisher;
        _integrationEventPublisher = integrationEventPublisher;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        if (_dbContext is not MessagingDbContextBase)
        {
            return;
        }

        var messages = await _dbContext
            .Set<OutboxMessage>()
            .Where(message => message.ProcessedOnUtc == null)
            .Take(20)
            .ToListAsync();

        if (messages.Count == 0)
        {
            return;
        }

        var messageTasks = messages.Select(message =>
            Task.Run(() =>
            {
                var messageType = Type.GetType(message.Type);
                var @event = JsonSerializer.Deserialize(message.Content, messageType!);
                message.ProcessedOnUtc = DateTime.UtcNow;

                if (message.MessageType == MessageType.DomainEvent)
                {
                    return _domainEventPublisher.Publish(@event!, context.CancellationToken);
                }
                else
                {
                    return _integrationEventPublisher.Publish(@event!, context.CancellationToken);
                }
            }, context.CancellationToken)
        );

        await Task.WhenAll(messageTasks);
        await _dbContext.SaveChangesAsync(context.CancellationToken);
    }
}
