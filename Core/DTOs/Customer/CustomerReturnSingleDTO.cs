namespace Core.DTOs.Customer
{
  public class CustomerReturnSingleDTO : CustomerReturnDTO
  {
    public float TotalDistanceDriven { get; set; }
    public float TotalPrice { get; set; }
  }
}
