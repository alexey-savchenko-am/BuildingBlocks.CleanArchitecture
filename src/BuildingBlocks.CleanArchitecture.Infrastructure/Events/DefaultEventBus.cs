using BuildingBlocks.CleanArchitecture.Application.Events;
using MassTransit;

namespace BuildingBlocks.CleanArchitecture.Infrastructure.Events;

public sealed class DefaultEventBus(IPublishEndpoint PublishEndpoint): IEventBus
{
    public async Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) 
        where T : IIntegrationEvent
    {
        await PublishEndpoint.Publish(message, cancellationToken);
    }
}
