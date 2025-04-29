using Core.DTOs.Customer;
using Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using rental_platform.Extentions;

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
    public async Task<IActionResult> Create([FromBody] CustomerCreateDTO customerCreateDTO)
    {
      if (!ModelState.IsValid)
        return BadRequest(ModelState);

      var result = await _customerService.Create(customerCreateDTO);

      if (result.IsSuccess)
        return CreatedAtAction(nameof(Get), new { id = result.Value.Id }, result.Value);

      return this.ToActionResult(result);

    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] CustomerUpdateDTO customerUpdateDTO)
    {
      if (!ModelState.IsValid)
        return BadRequest(ModelState);

      var result = await _customerService.Update(customerUpdateDTO);

      return this.ToActionResult(result);
    }
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
      var result = await _customerService.Delete(id);
      
      return this.ToActionResult(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
      var result = await _customerService.GetAll();

      return this.ToActionResult(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
      var result = await _customerService.GetById(id);

      return this.ToActionResult(result);
    }
  }
}
