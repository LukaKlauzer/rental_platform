using Application.DTOs.Vehicle;
using Application.Interfaces.Mapers;
using Application.Interfaces.Persistence.SpecificRepository;
using Application.Interfaces.Services;
using Core.Enums;
using Core.Result;

namespace Application.Services
{
  public class VehicleService : IVehicleService
  {
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IRentalRepository _rentalRepository;
    private readonly IVehicleMapper _vehicleMapper;
    public VehicleService(
      IVehicleRepository vehicleRepository,
      IRentalRepository rentalRepository,
      IVehicleMapper vehicleMapper)
    {
      _vehicleRepository = vehicleRepository;
      _rentalRepository = rentalRepository;
      _vehicleMapper = vehicleMapper;
    }

    public async Task<Result<List<VehicleReturnDto>>> GetAll()
    {
      var allVehiclesResult = await _vehicleRepository.GetAll();

      return allVehiclesResult.Match(
        vehicles => _vehicleMapper.ToReturnDtoList(vehicles),
        error => Result<List<VehicleReturnDto>>.Failure(error));
    }

    public async Task<Result<VehicleReturnSingleDto>> GetByVin(string vin)
    {
      var vehicleResult = await _vehicleRepository.GetByVin(vin);
      if (vehicleResult.IsFailure)
        return Result<VehicleReturnSingleDto>.Failure(vehicleResult.Error);

      var allRentalsResult = await _rentalRepository.GetByVin(vin);
      if (allRentalsResult.IsFailure)
        return Result<VehicleReturnSingleDto>.Failure(allRentalsResult.Error);

      // Convert now to handle cases with no rentals or incomplete ones
      var returnDtoResult = _vehicleMapper.ToReturnSingleDto(vehicleResult.Value);
      if (returnDtoResult.IsFailure)
        return Result<VehicleReturnSingleDto>.Failure(returnDtoResult.Error);

      if (!allRentalsResult.Value.Any())
        return Result<VehicleReturnSingleDto>.Success(returnDtoResult.Value);
      

      var completedRentals = allRentalsResult.Value.Where(rental =>
        rental.OdometerEnd.HasValue &&
        rental.BatterySOCEnd.HasValue &&
        rental.RentalStatus == RentalStatus.Ordered).ToList();

      if (!completedRentals.Any())
        return Result<VehicleReturnSingleDto>.Success(returnDtoResult.Value);

      // Calculate statistics
      var rentalStats = completedRentals.Select(rental =>
      {
        float distance = rental.OdometerEnd!.Value - rental.OdometerStart;
        int days = Math.Max(1, (int)Math.Ceiling((rental.EndDate - rental.StartDate).TotalDays));
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
      
      var totalDistanceDriven = rentalStats.Sum(d => d.Distance);
      var totalRentalCount = rentalStats.Count;
      var totalRentalIncome = rentalStats.Sum(d => d.Income);

      returnDtoResult = _vehicleMapper.ToReturnSingleDto(
        vehicleResult.Value, totalDistanceDriven, totalRentalCount, totalRentalIncome);
      
      return returnDtoResult;

    }
  }
}