using Application.DTOs.Rental;
using Core.Result;

namespace Application.Interfaces.DataValidation
{
  public interface IRentalValidator
  {
    Result<bool> ValidateCreate(RentalCreateDto dto);
    Result<bool> ValidateUpdate(RentalUpdateDto dto);
    Result<bool> ValidateCancle(int id);
    Result<bool> ValidateGetById(int id);
    Task<Result<bool>> ValidateNoOverlap(int customerId, string vehicleId, DateTime startDate, DateTime endDate, int? currentRentalId = null);
  }
}
