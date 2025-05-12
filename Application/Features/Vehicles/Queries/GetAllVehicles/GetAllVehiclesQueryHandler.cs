using Application.DTOs.Vehicle;
using Application.Interfaces.Mapers;
using Application.Interfaces.Persistence.SpecificRepository;
using Core.Result;
using MediatR;

namespace Application.Features.Vehicles.Queries.GetAllVehicles
{
  public class GetAllVehiclesQueryHandler : IRequestHandler<GetAllVehiclesQuery, Result<List<VehicleReturnDto>>>
  {
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IVehicleMapper _vehicleMapper;

    public GetAllVehiclesQueryHandler(
        IVehicleRepository vehicleRepository,
        IVehicleMapper vehicleMapper)
    {
      _vehicleRepository = vehicleRepository;
      _vehicleMapper = vehicleMapper;
    }

    public async Task<Result<List<VehicleReturnDto>>> Handle(GetAllVehiclesQuery request, CancellationToken cancellationToken)
    {
      var allVehiclesResult = await _vehicleRepository.GetAll(cancellationToken);

      return allVehiclesResult.Match(
          vehicles => _vehicleMapper.ToReturnDtoList(vehicles),
          error => Result<List<VehicleReturnDto>>.Failure(error));
    }
  }
}