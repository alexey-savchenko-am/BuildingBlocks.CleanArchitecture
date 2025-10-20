namespace BuildingBlocks.CleanArchitecture.Domain.Core;

public interface IEntityId
{
}

public interface IEntityId<out TKey> : IEntityId
{
    TKey Value { get; }
}