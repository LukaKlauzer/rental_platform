using Core.DTOs.Customer;
using Microsoft.AspNetCore.Mvc;

namespace rental_platform.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class CustomerController : ControllerBase
  {
    public CustomerController()
    {
    }
    [HttpPost]
    public IActionResult Create([FromBody] CustomerCreateDTO customerCreateDTO) 
    {
      return Ok();
    }

    [HttpPut]
    public IActionResult Update([FromBody] CustomerUpdateDTO customerUpdateDTO) 
    {
      return Ok();
    }
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
      return Ok();
    }

    [HttpGet]
    public IActionResult GetAll()
    {
      return Ok();
    }

    [HttpGet]
    public IActionResult Get([FromQuery] CustomerSearchDTO customerSearchDTO)
    {
      return Ok();
    }
  }
}
