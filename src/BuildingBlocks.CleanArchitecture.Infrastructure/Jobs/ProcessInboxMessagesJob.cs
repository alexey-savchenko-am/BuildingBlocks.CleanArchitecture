using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;
using BuildingBlocks.CleanArchitecture.Infrastructure.Persistence.Database.InboxOutbox;
using System.Text.Json.Serialization;

namespace BuildingBlocks.CleanArchitecture.Infrastructure.Jobs;

[DisallowConcurrentExecution]
public sealed class ProcessInboxMessagesJob : IJob
{
    private readonly DbContext _dbContext;
    private readonly ISender _mediator;
    private readonly ILogger<ProcessInboxMessagesJob> _logger;

    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public ProcessInboxMessagesJob(
        DbContext dbContext,
        ISender mediator,
        ILogger<ProcessInboxMessagesJob> logger)
    {
        _dbContext = dbContext;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        if (_dbContext is not MessagingDbContextBase messagingDbContext)
        {
            _logger.LogWarning("DbContext is not MessagingDbContextBase, skipping inbox job.");
            return;
        }

        var messages = await messagingDbContext
            .Set<InboxMessage>()
            .Where(x => x.ProcessedOnUtc == null)
            .OrderBy(x => x.ReceivedOnUtc)
            .Take(20)
            .ToListAsync(context.CancellationToken);

        if (messages.Count == 0)
        {
            _logger.LogDebug("No inbox messages to process.");
            return;
        }

        _logger.LogInformation("Processing {Count} inbox messages...", messages.Count);

        foreach (var message in messages)
        {
            try
            {
                var messageType = Type.GetType(message.Type);
                if (messageType is null)
                {
                    _logger.LogWarning("Inbox message type {Type} not found (ID: {Id})", message.Type, message.Id);
                    continue;
                }

                var payload = JsonSerializer.Deserialize(message.Content, messageType, _options);
                if (payload is null)
                {
                    _logger.LogWarning("Failed to deserialize inbox message {Id} of type {Type}", message.Id, message.Type);
                    continue;
                }

                await _mediator.Send(payload, context.CancellationToken);

                message.ProcessedOnUtc = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while processing inbox message {Id}", message.Id);
                message.Error = ex.Message;
                message.RetryCount++;
            }
        }

        await _dbContext.SaveChangesAsync(context.CancellationToken).ConfigureAwait(false);
        _logger.LogInformation("Inbox job completed. Processed {Count} messages.", messages.Count);
    }
}
