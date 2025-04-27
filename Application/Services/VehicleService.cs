using Core.DTOs.Vehicle;
using Core.Enums;
using Core.Extensions;
using Core.Interfaces.Persistence.SpecificRepository;
using Core.Interfaces.Services;
using Core.Result;

namespace Application.Services
{
  internal class VehicleService : IVeachelService
  {
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IRentalRepository _rentalRepository;
    public VehicleService(
      IVehicleRepository vehicleRepository,
      IRentalRepository rentalRepository)
    {
      _vehicleRepository = vehicleRepository;
      _rentalRepository = rentalRepository;
    }

    public async Task<Result<List<VehicleReturnDTO>>> GetAll()
    {
      var allVehiclesResoult = await _vehicleRepository.GetAll();

      return allVehiclesResoult.Match(
        vehicles => Result<List<VehicleReturnDTO>>.Success(vehicles.ToListReturnDTO()),
        error => Result<List<VehicleReturnDTO>>.Failure(error));
    }

    public async Task<Result<VehicleReturnSingleDTO>> GetByVin(string vin)
    {
      var vehicleResult = await _vehicleRepository.GetByVin(vin);
      if (vehicleResult.IsFailure)
        return Result<VehicleReturnSingleDTO>.Failure(vehicleResult.Error);

      var returnDto = vehicleResult.Value.ToSingleResultDTO();
      if (returnDto is null)
        return Result<VehicleReturnSingleDTO>.Failure(Error.MappingError("Failed to map vehicle entity to return DTO "));

      var allRentalsResoult = await _rentalRepository.GetByVin(vin);
      if (allRentalsResoult.IsFailure)
        return Result<VehicleReturnSingleDTO>.Failure(allRentalsResoult.Error);

      if (!allRentalsResoult.Value.Any())
        return Result<VehicleReturnSingleDTO>.Success(returnDto);

      var completedRentals = allRentalsResoult.Value.Where(rental =>
        rental.OdometerEnd.HasValue &&
        rental.BatterySOCEnd.HasValue &&
        rental.RentalStatus == RentalStatus.Ordered).ToList();

      if (!completedRentals.Any())
        return Result<VehicleReturnSingleDTO>.Success(returnDto);

      // Calculate statistics
      var rentalStats = completedRentals.Select(rental =>
      {
        float distance = rental.OdometerEnd!.Value - rental.OdometerStart;
        int days = (int)Math.Ceiling((rental.EndDate - rental.StartDate).TotalDays);
        float batteryDelta = rental.BatterySOCEnd!.Value - rental.BatterySOCStart;

        float distanceCost = distance * vehicleResult.Value.PricePerKmInEuro;
        float dailyCost = days * vehicleResult.Value.PricePerDayInEuro;
        float batteryPenalty = Math.Max(0, -batteryDelta) * 0.2f;

        return new
        {
          Distance = distance,
          Income = distanceCost + dailyCost + batteryPenalty
        };
      }).ToList();

      returnDto.TotalDistanceDriven = rentalStats.Sum(d => d.Distance);
      returnDto.TotalRentalCount = rentalStats.Count;
      returnDto.TotalRentalIncome = rentalStats.Sum(d => d.Income);

      return Result<VehicleReturnSingleDTO>.Success(returnDto);

    }
  }
}