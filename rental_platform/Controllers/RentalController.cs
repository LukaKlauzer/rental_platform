using Application.Features.Rentals.Commands.CancelReservation;
using Application.Features.Rentals.Commands.CreateReservation;
using Application.Features.Rentals.Commands.UpdateReservation;
using Application.Features.Rentals.Queries.GetAllReservations;
using Application.Features.Rentals.Queries.GetReservationById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rental_platform.Extensions;

namespace rental_platform.Controllers
{
  [Authorize]
  [ApiController]
  [Route("api/[controller]")]
  public class RentalController : ControllerBase
  {
    public readonly IMediator _mediator;
    public RentalController(IMediator mediator)
    {
      _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReservationCommand command)
    {
      var result = await _mediator.Send(command);

      return this.ToActionResult(result);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateReservationCommand command)
    {
      var result = await _mediator.Send(command);

      return this.ToActionResult(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Cancel(int id)
    {
      var command = new CancelReservationCommand(id);
      var result = await _mediator.Send(command);

      return this.ToActionResult(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
      var query = new GetAllReservationsQuery();
      var result = await _mediator.Send(query);

      return this.ToActionResult(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
      var query = new GetReservationByIdQuery(id);
      var result = await _mediator.Send(query);

      return this.ToActionResult(result);
    }
  }
}
