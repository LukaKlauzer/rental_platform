using Core.DTOs.Vehicle;
using Core.Features.Vehicle.Queries;
using Core.Interfaces.Persistence.SpecificRepository;
using Core.Result;
using Core.Extensions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Vehicle.Queries
{
  internal class GetAllVehiclesQueryHandler : IRequestHandler<GetAllVehiclesQuery, Result<List<VehicleReturnDTO>>>
                                                              
  {
    private readonly ILogger<GetAllVehiclesQueryHandler> _logger;
    private readonly IVehicleRepository _vehicleRepository;

    public GetAllVehiclesQueryHandler(
        ILogger<GetAllVehiclesQueryHandler> logger,
        IVehicleRepository vehicleRepository)
    {
      _logger = logger;
      _vehicleRepository = vehicleRepository;
    }

    public async Task<Result<List<VehicleReturnDTO>>> Handle(GetAllVehiclesQuery request, CancellationToken cancellationToken)
    {
      var allVehiclesResult = await _vehicleRepository.GetAll(cancellationToken);

      if (allVehiclesResult.IsFailure)
      {
        _logger.LogError("Failed to retrieve all vehicles: {ErrorMessage}",
            allVehiclesResult.Error.Message);
        return Result<List<VehicleReturnDTO>>.Failure(allVehiclesResult.Error);
      }

      _logger.LogInformation("Successfully retrieved {VehicleCount} vehicles",
          allVehiclesResult.Value.Count);

      return allVehiclesResult.Match(
          vehicles => Result<List<VehicleReturnDTO>>.Success(vehicles.ToListReturnDTO()),
          error => Result<List<VehicleReturnDTO>>.Failure(error)
      );
    }
  
  }
}
