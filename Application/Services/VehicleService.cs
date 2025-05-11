using Application.DTOs.Vehicle;
using Application.Interfaces.DataValidation;
using Application.Interfaces.Mapers;
using Application.Interfaces.Persistence.SpecificRepository;
using Application.Interfaces.Services;
using Core.Result;

namespace Application.Services
{
  public class VehicleService : IVehicleService
  {
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IVehicleMapper _vehicleMapper;
    private readonly IVehicleValidator _vehicleValidator; 
    public VehicleService(
      IVehicleRepository vehicleRepository,
      IVehicleMapper vehicleMapper,
      IVehicleValidator vehicleValidator)
    {
      _vehicleRepository = vehicleRepository;
      _vehicleMapper = vehicleMapper;
      _vehicleValidator = vehicleValidator;
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
      var validationResult = _vehicleValidator.ValidateGetByVin(vin);
      if (validationResult.IsFailure)
        return Result<VehicleReturnSingleDto>.Failure(validationResult.Error);

      var vehicleResult = await _vehicleRepository.GetByVinWithRentals(vin);
      if (vehicleResult.IsFailure)
        return Result<VehicleReturnSingleDto>.Failure(vehicleResult.Error);

      var vehicle = vehicleResult.Value;

      if (!vehicle.Rentals.Any() || !vehicle.Rentals.Any(r => r.IsCompleted()))
        return _vehicleMapper.ToReturnSingleDto(vehicle);
      
      var statisticsResult = vehicle.CalculateStatistics();
      if (statisticsResult.IsFailure)
        return Result<VehicleReturnSingleDto>.Failure(statisticsResult.Error);

      var statistics = statisticsResult.Value;

      return _vehicleMapper.ToReturnSingleDto(
          vehicle,
          statistics.TotalDistanceDriven,
          statistics.TotalRentalCount,
          statistics.TotalRentalIncome);

    }
  }
}