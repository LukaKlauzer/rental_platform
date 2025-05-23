using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rental_platform.Extensions;

namespace rental_platform.Controllers
{
  [Authorize]
  [ApiController]
  [Route("api/[controller]")]
  public class VehicleController : ControllerBase
  {
    private readonly IVehicleService _vehicleService;
    public VehicleController(IVehicleService vehicleService)
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