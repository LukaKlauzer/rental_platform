using Core.DTOs.Vehicle;
using Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using rental_platform.Extentions;

namespace rental_platform.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class VehicleController : ControllerBase
  {
    private readonly IVeachelService _vehicleService;
    public VehicleController(IVeachelService vehicleService)
    {
      _vehicleService = vehicleService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
      var result = await _vehicleService.GetAll();

      return this.ToActionResult(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id)
    {
      var result = await _vehicleService.GetByVin(id);

      return this.ToActionResult(result);
    }
  }
}