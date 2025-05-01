using Core.DTOs.Vehicle;
using Core.Result;
using MediatR;

namespace Core.Features.Vehicle.Queries
{
  public record GetByIdVehicleQuery(string Vin) : IRequest<Result<VehicleReturnSingleDTO>>;
}
