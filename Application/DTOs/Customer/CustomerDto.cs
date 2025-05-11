namespace Application.DTOs.Customer
{
  public record CustomerCreateDto(string Name);
  public record CustomerUpdateDto(int Id, string Name);
  public record CustomerReturnDto(int Id, string Name, bool IsDeleted);
  public record CustomerReturnSingleDto(
    int Id,
    string Name,
    bool IsDeleted,
    float TotalDistanceDriven,
    float TotalPrice);

}
