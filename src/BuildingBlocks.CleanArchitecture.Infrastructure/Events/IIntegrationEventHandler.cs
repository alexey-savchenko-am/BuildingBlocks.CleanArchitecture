using BuildingBlocks.CleanArchitecture.Application.Events;
using MassTransit;

namespace BuildingBlocks.CleanArchitecture.Infrastructure.Events;

public interface IIntegrationEventHandler<TEvent>
    : IConsumer<TEvent>
    where TEvent: class, IIntegrationEvent
{
}
