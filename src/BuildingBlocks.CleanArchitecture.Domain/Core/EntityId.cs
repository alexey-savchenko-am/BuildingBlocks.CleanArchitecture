namespace BuildingBlocks.CleanArchitecture.Domain.Core;


public abstract record GuidEntityId<TSelf>(Guid Value)
    : EntityId<TSelf, Guid>(Value)
    where TSelf : EntityId<TSelf, Guid>
{
}

public abstract record StringEntityId<TSelf>(string Value)
    : EntityId<TSelf, string>(Value)
    where TSelf : EntityId<TSelf, string>
{
}

public abstract record EntityId<TSelf, TValue>
    : IEquatable<EntityId<TSelf, TValue>>
    , IEntityId<TValue>
    where TSelf : EntityId<TSelf, TValue>
{
    public TValue Value { get; }

    protected EntityId(TValue value)
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));

        Value = value;
    }

    public override string ToString() => Value?.ToString() ?? string.Empty;

    public virtual bool Equals(EntityId<TSelf, TValue>? other) =>
        other is not null && EqualityComparer<TValue>.Default.Equals(Value, other.Value);

    public override int GetHashCode() =>
        EqualityComparer<TValue>.Default.GetHashCode(Value!);
}