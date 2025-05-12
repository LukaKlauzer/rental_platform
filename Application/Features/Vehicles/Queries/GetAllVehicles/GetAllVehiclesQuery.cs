using Application.DTOs.Vehicle;
using Core.Result;
using MediatR;

namespace Application.Features.Vehicles.Queries.GetAllVehicles
{
  public record GetAllVehiclesQuery : IRequest<Result<List<VehicleReturnDto>>>;
}