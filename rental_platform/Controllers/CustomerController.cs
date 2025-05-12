using Application.Features.Customer.Commands.CreateCustomer;
using Application.Features.Customer.Commands.DeleteCustomer;
using Application.Features.Customer.Commands.UpdateCustomer;
using Application.Features.Customer.Queries.GetAllCustomers;
using Application.Features.Customer.Queries.GetCustomerById;
using Application.Features.Customer.Queries.LoginCustomer;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rental_platform.Extensions;

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
    public async Task<IActionResult> Create([FromBody] CreateCustomerCommand command)
    {
      var result = await _mediator.Send(command);

      return this.ToActionResult(result);
    }

    [HttpGet("login/{id}")]
    public async Task<IActionResult> Login(int id)
    {
      var querry = new LoginCustomerQuery(id);

      var result = await _mediator.Send(querry);

      return this.ToActionResult(result);
    }

    [Authorize]
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateCustomerCommand command)
    {
      var result = await _mediator.Send(command);

      return this.ToActionResult(result);
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
      var command = new DeleteCustomerCommand(id);
      var result = await _mediator.Send(command);

      return this.ToActionResult(result);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
      var querry = new GetAllCustomersQuery();
      var result = await _mediator.Send(querry);

      return this.ToActionResult(result);
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
      var querry = new GetCustomerByIdQuery(id);
      var result = await _mediator.Send(querry);

      return this.ToActionResult(result);
    }
  }
}
