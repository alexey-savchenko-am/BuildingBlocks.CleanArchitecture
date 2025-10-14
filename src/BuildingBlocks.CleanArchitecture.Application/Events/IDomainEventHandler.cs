using BuildingBlocks.CleanArchitecture.Domain;
using MediatR;

namespace BuildingBlocks.CleanArchitecture.Application.Events;

public interface IDomainEventHandler<TEvent>
    : INotificationHandler<TEvent>
    where TEvent : IDomainEvent
{
}
