using Core.Features.Vehicle.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rental_platform.Extentions;

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
      var result = await _mediator.Send(new GetAllVehiclesQuery());
      return this.ToActionResult(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id)
    {
      var result = await _mediator.Send(new GetByIdVehicleQuery(id));
      return this.ToActionResult(result);
    }
  }
}