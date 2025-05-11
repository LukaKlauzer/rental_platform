namespace Application.DTOs.Vehicle
{
  public record VehicleCreateDto(
    string Vin,
    string Make,
    string Model,
    DateOnly Year,
    float PricePerKmInEuro,
    float PricePerDayInEuro);
  public record VehicleReturnDto(
    string Vin,
    string Make,
    string Model,
    int Year,
    float PricePerKmInEuro,
    float PricePerDayInEuro);

  public record VehicleReturnSingleDto(
     string Vin,
     string Make,
     string Model,
     int Year,
     float PricePerKmInEuro,
     float PricePerDayInEuro,

     float TotalDistanceDriven,
     int TotalRentalCount,
     float TotalRentalIncome);
}
