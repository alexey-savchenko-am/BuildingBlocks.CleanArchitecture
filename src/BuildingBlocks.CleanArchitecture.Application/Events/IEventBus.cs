namespace BuildingBlocks.CleanArchitecture.Application.Events;

public interface IEventBus
{
    Task PublishAsync<T>(T message, CancellationToken cancellationToken = default)
        where T : IIntegrationEvent;
}
