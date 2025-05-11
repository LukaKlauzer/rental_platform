using Application.DTOs.Customer;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rental_platform.Extensions;

namespace rental_platform.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class CustomerController : ControllerBase
  {
    private readonly ICustomerService _customerService;
    public CustomerController(ICustomerService customerService)
    {
      _customerService = customerService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CustomerCreateDto customerCreateDTO)
    {
      var result = await _customerService.Create(customerCreateDTO);

      return this.ToActionResult(result);
    }

    [HttpGet("login/{id}")]
    public async Task<IActionResult> Login(int id)
    {
      var result = await _customerService.Login(id);

      return this.ToActionResult(result);
    }

    [Authorize]
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] CustomerUpdateDto customerUpdateDTO)
    {
      var result = await _customerService.Update(customerUpdateDTO);

      return this.ToActionResult(result);
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
      var result = await _customerService.Delete(id);

      return this.ToActionResult(result);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
      var result = await _customerService.GetAll();

      return this.ToActionResult(result);
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
      var result = await _customerService.GetById(id);

      return this.ToActionResult(result);
    }
  }
}
