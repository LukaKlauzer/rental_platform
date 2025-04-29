namespace Core.DTOs.Customer
{
  public class CustomerReturnDTO
  {
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
  }
}
