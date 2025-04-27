using Core.DTOs.Rental;
using Core.Result;

namespace Core.Interfaces.Services
{
  public interface IRentalService
  {
    public Task<Result<RentalReturnDTO>> CreateReservation(RentalCreateDTO rentalCreateDTO);
    public Task<Result<bool>> CancelReservation(int id);
    public Task<Result<bool>> UpdateReservation(RentalUpdateDTO rentalUpdateDTO);
    public Task<Result<List<RentalReturnDTO>>> GetAll();
    public Task<Result<RentalReturnSingleDTO>> GetById(int id);
  }
}
