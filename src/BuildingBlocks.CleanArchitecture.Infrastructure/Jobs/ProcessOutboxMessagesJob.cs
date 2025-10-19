using BuildingBlocks.CleanArchitecture.Infrastructure.Persistence.Database.InboxOutbox;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BuildingBlocks.CleanArchitecture.Infrastructure.Jobs;

[DisallowConcurrentExecution]
public sealed class ProcessOutboxMessagesJob : IJob
{
    private readonly DbContext _dbContext;
    private readonly IPublisher _domainEventPublisher;
    private readonly IPublishEndpoint _integrationEventPublisher;
    private readonly ILogger<ProcessOutboxMessagesJob> _logger;

    public ProcessOutboxMessagesJob(
        DbContext dbContext,
        IPublisher domainEventPublisher,
        IPublishEndpoint integrationEventPublisher,
        ILogger<ProcessOutboxMessagesJob> logger)
    {
        _dbContext = dbContext;
        _domainEventPublisher = domainEventPublisher;
        _integrationEventPublisher = integrationEventPublisher;
        _logger = logger;
    }

    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public async Task Execute(IJobExecutionContext context)
    {
        if (_dbContext is not MessagingDbContextBase)
        {
            _logger.LogWarning("DbContext is not MessagingDbContext, skipping outbox job.");
            return;
        }

        var messages = await _dbContext
            .Set<OutboxMessage>()
            .Where(message => message.ProcessedOnUtc == null)
            .Take(20)
            .ToListAsync();

        if (messages.Count == 0)
        {
            _logger.LogDebug("No outbox messages to process.");
            return;
        }

        _logger.LogInformation("Processing {Count} outbox messages...", messages.Count);

        var tasks = messages.Select(async message =>
        {
            try
            {
                var messageType = Type.GetType(message.Type);
                if(messageType is null)
                {
                    _logger.LogWarning("Message type {Type} not found. Skipping message {Id}", message.Type, message.Id);
                    return;
                }

                var @event = JsonSerializer.Deserialize(message.Content, messageType!, _options);
                if(@event is null)
                {
                    _logger.LogWarning("Failed to deserialize message {Id} of type {Type}", message.Id, message.Type);
                    return;
                }

                if (message.MessageType == MessageType.DomainEvent)
                    await _domainEventPublisher.Publish(@event!, context.CancellationToken);
                else
                    await _integrationEventPublisher.Publish(@event!, context.CancellationToken);

                message.ProcessedOnUtc = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while outbox message {Id}", message.Id);
                message.Error = ex.Message;
                message.RetryCount++;
            }
        });

        await Task.WhenAll(tasks).ConfigureAwait(false); ;
        await _dbContext.SaveChangesAsync(context.CancellationToken).ConfigureAwait(false); ;
        _logger.LogInformation("Outbox job completed. Processed {Count} messages.", messages.Count);
    }
}
