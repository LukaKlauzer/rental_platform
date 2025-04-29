using Core.DTOs.Rental;
using Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using rental_platform.Extentions;

namespace rental_platform.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class RentalController : ControllerBase
  {
    public readonly IRentalService _rentalService;
    public RentalController(IRentalService rentalService)
    {
      _rentalService = rentalService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] RentalCreateDTO rentalCreateDTO)
    {
      if (!ModelState.IsValid)
        return BadRequest(ModelState);

      var result = await _rentalService.CreateReservation(rentalCreateDTO);

      if (result.IsSuccess)
        return CreatedAtAction(nameof(Get), new { id = result.Value.Id }, result.Value);

      return this.ToActionResult(result);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] RentalUpdateDTO rentalUpdateDTO)
    {
      if (!ModelState.IsValid)
        return BadRequest(ModelState);

      var result = await _rentalService.UpdateReservation(rentalUpdateDTO);

      return this.ToActionResult(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Cancle(int id)
    {
      var result = await _rentalService.CancelReservation(id);

      return this.ToActionResult(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
      var result = await _rentalService.GetAll();

      return this.ToActionResult(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
      var result = await _rentalService.GetById(id);

      return this.ToActionResult(result);
    }
  }
}
