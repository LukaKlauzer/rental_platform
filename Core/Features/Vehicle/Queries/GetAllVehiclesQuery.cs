using Core.DTOs.Vehicle;
using Core.Result;
using MediatR;

namespace Core.Features.Vehicle.Queries
{
  public record GetAllVehiclesQuery : IRequest<Result<List<VehicleReturnDTO>>>;
}
