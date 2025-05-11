using Core.Enums;

namespace Application.DTOs.Rental
{
  public record RentalCreateDto(
      DateTime StartDate,
      DateTime EndDate,
      string VehicleId,
      int CustomerId);

  public record RentalReturnDto(
       int Id,
       DateTime StartDate,
       DateTime EndDate,
       RentalStatus RentalStatus,
       string VehicleId,
       int CustomerId);

  public record RentalReturnSingleDto(
       int Id,
       DateTime StartDate,
       DateTime EndDate,
       RentalStatus RentalStatus,
       string VehicleId,
       int CustomerId,

       float? DistanceTraveled,
       float BatterySOCSAtStart,
       float? BatterySOCAtEnd);

  public record RentalUpdateDto(
     int Id,
     DateTime? StartDate,
     DateTime? EndDate);
}
