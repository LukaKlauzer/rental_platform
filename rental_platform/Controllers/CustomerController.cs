using Core.DTOs.Customer;
using Core.Features.Customer.Queries;
using Core.Features.Rental.Commands;
using Core.Features.Rental.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rental_platform.Extentions;

namespace rental_platform.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class CustomerController : ControllerBase
  {
    private readonly IMediator _mediator;
    public CustomerController(IMediator mediator)
    {
      _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRentalCommand command)
    {
      var result = await _mediator.Send(command);

      return this.ToActionResult(result);
    }

    [HttpGet("login/{id}")]
    public async Task<IActionResult> Login(int id)
    {
      var result = await _mediator.Send(new LoginCustomerQuery(id));

      return this.ToActionResult(result);
    }

    [Authorize]
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

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
      var result = await _mediator.Send(new GetAllRentalsQuery());
      return this.ToActionResult(result);
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
      var result = await _mediator.Send(new GetByIdRentalQuery(id));
      return this.ToActionResult(result);
    }
  }
}
