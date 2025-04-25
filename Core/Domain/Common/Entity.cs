namespace Core.Domain.Common
{
  public class Entity
  {
    public DateTime DateCreated { get; set; }
    public DateTime DateModified { get; set; }
  }

  public class EntityID: Entity
  {
    public int ID { get; set; }
  }
}
