namespace BuildingBlocks.CleanArchitecture.Domain.Core;


public abstract class GuidKeyEntity<TId>
    : Entity<TId, Guid>
    where TId : GuidEntityId<TId>
{
    protected GuidKeyEntity(TId id) 
        : base(id)
    {
    }
}

public abstract class StringKeyEntity<TId>
    : Entity<TId, string>
    where TId : StringEntityId<TId>
{
    protected StringKeyEntity(TId id)
        : base(id)
    {
    }
}

public abstract class Entity<TId, TKey>
    : IEquatable<Entity<TId, TKey>>
    where TId : EntityId<TId, TKey>
{
    public TId Id { get; }

    protected Entity(TId id) => Id = id;

    public bool Equals(Entity<TId, TKey>? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;

        return Equals((Entity<TId, TKey>)obj);
    }

    public override int GetHashCode()
    {
        return EqualityComparer<TId>.Default.GetHashCode();
    }

    public static bool operator ==(Entity<TId, TKey>? left, Entity<TId, TKey>? right) =>
    Equals(left, right);

    public static bool operator !=(Entity<TId, TKey>? left, Entity<TId, TKey>? right) =>
        !Equals(left, right);
}