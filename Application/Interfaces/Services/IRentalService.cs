using Application.DTOs.Rental;
using Core.Result;

namespace Application.Interfaces.Services
{
  public interface IRentalService
  {
    public Task<Result<RentalReturnDto>> CreateReservation(RentalCreateDto rentalCreateDTO);
    public Task<Result<bool>> CancelReservation(int id);
    public Task<Result<bool>> UpdateReservation(RentalUpdateDto rentalUpdateDTO);
    public Task<Result<List<RentalReturnDto>>> GetAll();
    public Task<Result<RentalReturnSingleDto>> GetById(int id);
  }
}
