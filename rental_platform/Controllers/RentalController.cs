using Core.DTOs.Rental;
using Core.Features.Rental.Commands;
using Core.Features.Rental.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rental_platform.Extentions;

namespace rental_platform.Controllers
{
  [Authorize]
  [ApiController]
  [Route("api/[controller]")]
  public class RentalController : ControllerBase
  {
    private readonly IMediator _mediator;
    public RentalController(IMediator mediator)
    {
      _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRentalCommand command)
    {
      var result = await _mediator.Send(command);

      return this.ToActionResult(result);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateRentalCommand command)
    {
      var result = await _mediator.Send(command);
      return this.ToActionResult(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Cancel(int id)
    {
      var result = await _mediator.Send(new CancelRentalCommand(id));
      return this.ToActionResult(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
      var result = await _mediator.Send(new GetAllRentalsQuery());
      return this.ToActionResult(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
      var result = await _mediator.Send(new GetByIdRentalQuery(id));
      return this.ToActionResult(result);
    }
  }
}
