using Application.DTOs.Rental;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rental_platform.Extensions;

namespace rental_platform.Controllers
{
  [Authorize]
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
    public async Task<IActionResult> Create([FromBody] RentalCreateDto rentalCreateDTO)
    {
      var result = await _rentalService.CreateReservation(rentalCreateDTO);

      if (result.IsSuccess)
        return CreatedAtAction(nameof(Get), new { id = result.Value.Id }, result.Value);

      return this.ToActionResult(result);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] RentalUpdateDto rentalUpdateDTO)
    {
      var result = await _rentalService.UpdateReservation(rentalUpdateDTO);

      return this.ToActionResult(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Cancel(int id)
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
