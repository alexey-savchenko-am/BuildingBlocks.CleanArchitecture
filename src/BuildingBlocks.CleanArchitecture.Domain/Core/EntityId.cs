namespace BuildingBlocks.CleanArchitecture.Domain.Core;


public abstract record GuidEntityId<TSelf>
    : EntityId<TSelf, Guid>
    where TSelf : EntityId<TSelf, Guid>
{
    protected GuidEntityId(Guid value) 
        : base(value)
    {
    }
}

public abstract record StringEntityId<TSelf>
    : EntityId<TSelf, string>
    where TSelf : EntityId<TSelf, string>
{
    protected StringEntityId(string value) 
        : base(value)
    {
    }
}

public abstract record EntityId<TSelf, TValue>
    : IEquatable<EntityId<TSelf, TValue>>
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

public interface IEntityId<TSelf, TValue>
    where TSelf : EntityId<TSelf, TValue>
{
    static abstract TSelf Create(TValue id);
}