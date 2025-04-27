using Core.DTOs.Vehicle;
using Microsoft.AspNetCore.Mvc;

namespace rental_platform.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class VehicleController : ControllerBase
  {
    public VehicleController()
    {
    }

    [HttpGet]
    public IActionResult GetAll()
    {
      return Ok();
    }

    [HttpGet("{id}")]
    public IActionResult Get(int id)
    {
      return Ok();
    }
  }
}