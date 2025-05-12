using Application.DTOs.Vehicle;
using Core.Result;
using MediatR;

namespace Application.Features.Vehicles.Queries.GetVehiclesByVin
{
  public record GetVehicleByVinQuery(string Vin) : IRequest<Result<VehicleReturnSingleDto>>;
}