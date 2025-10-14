namespace BuildingBlocks.CleanArchitecture.Application.Events;

public interface IIntegrationEvent
{
    Guid Id { get; }

    DateTime OccurredOnUtc { get; }
}
