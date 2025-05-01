using Core.DTOs.Vehicle;
using Core.Enums;
using Core.Features.Vehicle.Queries;
using Core.Interfaces.Persistence.SpecificRepository;
using Core.Result;
using Core.Extensions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Vehicle.Queries
{
  internal class GetByIdVehicleQueryHandler : IRequestHandler<GetByIdVehicleQuery, Result<VehicleReturnSingleDTO>>
  {
    private readonly ILogger<GetByIdVehicleQueryHandler> _logger;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IRentalRepository _rentalRepository;

    public GetByIdVehicleQueryHandler(
        ILogger<GetByIdVehicleQueryHandler> logger,
        IVehicleRepository vehicleRepository,
        IRentalRepository rentalRepository)
    {
      _logger = logger;
      _vehicleRepository = vehicleRepository;
      _rentalRepository = rentalRepository;
    }

    public async Task<Result<VehicleReturnSingleDTO>> Handle(GetByIdVehicleQuery request, CancellationToken cancellationToken)
    {
      var vehicleResult = await _vehicleRepository.GetByVin(request.Vin, cancellationToken);
      if (vehicleResult.IsFailure)
      {
        _logger.LogWarning("Failed to retrieve vehicle with VIN {VIN}: {ErrorMessage}",
            request.Vin, vehicleResult.Error.Message);
        return Result<VehicleReturnSingleDTO>.Failure(vehicleResult.Error);
      }

      var returnDto = vehicleResult.Value.ToSingleResultDTO();
      if (returnDto is null)
      {
        _logger.LogError("Failed to map vehicle entity to return DTO for VIN {VIN}", request.Vin);
        return Result<VehicleReturnSingleDTO>.Failure(
            Error.MappingError("Failed to map vehicle entity to return DTO"));
      }

      var allRentalsResult = await _rentalRepository.GetByVin(request.Vin, cancellationToken);
      if (allRentalsResult.IsFailure)
      {
        _logger.LogWarning("Failed to retrieve rentals for vehicle with VIN {VIN}: {ErrorMessage}",
            request.Vin, allRentalsResult.Error.Message);
        return Result<VehicleReturnSingleDTO>.Failure(allRentalsResult.Error);
      }

      if (!allRentalsResult.Value.Any())
      {
        _logger.LogInformation("No rentals found for vehicle with VIN {VIN}", request.Vin);
        return Result<VehicleReturnSingleDTO>.Success(returnDto);
      }

      var completedRentals = allRentalsResult.Value.Where(rental =>
          rental.OdometerEnd.HasValue &&
          rental.BatterySOCEnd.HasValue &&
          rental.RentalStatus == RentalStatus.Ordered).ToList();

      if (!completedRentals.Any())
      {
        _logger.LogInformation("No completed rentals found for vehicle with VIN {VIN}", request.Vin);
        return Result<VehicleReturnSingleDTO>.Success(returnDto);
      }

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

      returnDto.TotalDistanceDriven = rentalStats.Sum(d => d.Distance);
      returnDto.TotalRentalCount = rentalStats.Count;
      returnDto.TotalRentalIncome = rentalStats.Sum(d => d.Income);

      _logger.LogInformation("Successfully retrieved vehicle statistics for VIN {VIN}", request.Vin);
      return Result<VehicleReturnSingleDTO>.Success(returnDto);
    }
  }
}
