using Application.Features.Vehicles.Queries.GetAllVehicles;
using Application.Features.Vehicles.Queries.GetVehiclesByVin;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rental_platform.Extensions;

namespace rental_platform.Controllers
{
  [Authorize]
  [ApiController]
  [Route("api/[controller]")]
  public class VehicleController : ControllerBase
  {
    private readonly IMediator _mediator;
    public VehicleController(IMediator mediator)
    {
      _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
      var query = new GetAllVehiclesQuery();
      var result = await _mediator.Send(query);

      return this.ToActionResult(result);
    }

    [HttpGet("{vin}")]
    public async Task<IActionResult> Get(string vin)
    {
      var query = new GetVehicleByVinQuery(vin);
      var result = await _mediator.Send(query);

      return this.ToActionResult(result);
    }
  }
}