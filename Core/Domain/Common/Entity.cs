namespace Core.Domain.Common
{
  public class Entity
  {
    public DateTime DateCreated { get; set; }
    public DateTime DateModified { get; set; }
  }

  public class EntityID : Entity, IEquatable<EntityID>
  {
    public int ID { get; protected set; }

    protected EntityID() { }
    protected EntityID(int id)
    {
      if (id <= 0)
        throw new ArgumentException("Entity ID must be positive", nameof(id));
      
      ID = id;
    }
    public override bool Equals(Object? obj)
    {

      if (obj is null) return false;
      if (obj.GetType() != GetType())
        return false;
      if (obj is not EntityID entity)
        return false;

      return ID == entity.ID;
    }

    public bool Equals(EntityID? other)
    {
      if (other is null)
        return false;

      if (other.GetType() != GetType())
        return false;

      return ID == other.ID;
    }

    public static bool operator ==(EntityID? first, EntityID? second) =>
      first is not null && second is not null && first.Equals(second);

    public static bool operator !=(EntityID first, EntityID? second) =>
       first is not null && second is null && !first.Equals(second);

  }
}
