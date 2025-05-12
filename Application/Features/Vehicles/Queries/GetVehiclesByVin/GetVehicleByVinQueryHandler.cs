using Application.DTOs.Vehicle;
using Application.Interfaces.Mapers;
using Application.Interfaces.Persistence.SpecificRepository;
using Core.Result;
using MediatR;

namespace Application.Features.Vehicles.Queries.GetVehiclesByVin
{
  public class GetVehicleByVinQueryHandler : IRequestHandler<GetVehicleByVinQuery, Result<VehicleReturnSingleDto>>
  {
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IVehicleMapper _vehicleMapper;

    public GetVehicleByVinQueryHandler(
        IVehicleRepository vehicleRepository,
        IVehicleMapper vehicleMapper)
    {
      _vehicleRepository = vehicleRepository;
      _vehicleMapper = vehicleMapper;
    }

    public async Task<Result<VehicleReturnSingleDto>> Handle(GetVehicleByVinQuery request, CancellationToken cancellationToken)
    {
      var vehicleResult = await _vehicleRepository.GetByVinWithRentals(request.Vin, cancellationToken);
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