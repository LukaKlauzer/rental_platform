using Core.DTOs.Rental;
using Microsoft.AspNetCore.Mvc;

namespace rental_platform.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class RentalController : ControllerBase
  {
    public RentalController()
    {
    }

    [HttpPost]
    public IActionResult Create([FromBody] RentalCreateDTO rentalCreateDTO) 
    {
      return Ok();
    }

    [HttpPut]
    public IActionResult Update([FromBody] RentalUpdateDTO rentalUpdateDTO ) 
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
    public IActionResult Get([FromQuery] RentalSearchDTO rentalSearchDTO)
    {
      return Ok();
    }
  }
}
