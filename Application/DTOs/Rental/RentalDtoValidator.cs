using Application.Interfaces.DataValidation;
using Application.Interfaces.Persistence.SpecificRepository;
using Core.Result;
using Microsoft.Extensions.Logging;

namespace Application.DTOs.Rental
{
  public class RentalDtoValidator : IRentalValidator
  {
    private readonly ILogger<RentalDtoValidator> _logger;
    private readonly IRentalRepository _rentalRepository;

    public RentalDtoValidator(
      ILogger<RentalDtoValidator> logger,
      IRentalRepository rentalRepository ) 
    {
      _logger = logger;
      _rentalRepository = rentalRepository;
    }

    public Result<bool> ValidateCreate(RentalCreateDto dto)
    {
      if (dto.CustomerId <= 0)
        return Result<bool>.Failure(Error.ValidationError($"Customer id is not valid: {dto.CustomerId}"));

      if (string.IsNullOrEmpty(dto.VehicleId))
        return Result<bool>.Failure(Error.ValidationError($"Vehicle vin is not valid: {dto.VehicleId}"));

      return Result<bool>.Success(true);
    }
    public Result<bool> ValidateUpdate(RentalUpdateDto dto)
    {
      if (dto.Id <= 0)
      {
        _logger.LogWarning("Rental update validation failed: Invalid ID ({RentalId})", dto.Id);
        return Result<bool>.Failure(Error.ValidationError($"Rental id is not valid: {dto.Id}"));
      }
      if (dto.StartDate is null && dto.EndDate is null)
      {
        _logger.LogWarning("Rental update validation failed: No update fields provided for rental {RentalId}", dto.Id);
        return Result<bool>.Failure(Error.ValidationError("At least one field must be provided for update"));
      }
      if (!dto.StartDate.HasValue && !dto.EndDate.HasValue)
        return Result<bool>.Failure(Error.ValidationError("Both start date and end date cannot be null for reservation update"));

      return Result<bool>.Success(true);
    }
    public Result<bool> ValidateCancle(int id) => ValidateGetById(id);
    public Result<bool> ValidateGetById(int id)
    {
      if (id <= 0)
      {
        _logger.LogWarning("Rental id validation failed: Invalid ID ({RentalId})", id);
        return Result<bool>.Failure(Error.ValidationError($"Rental id is not valid: {id}"));
      }
      return Result<bool>.Success(true);
    }
    public async Task<Result<bool>> ValidateNoOverlap(
      int customerId,
      string vehicleId,
      DateTime startDate,
      DateTime endDate,
      int? currentRentalId = null)
    {
      var customerReservationsInTimeFrameResult = await _rentalRepository.GetByCustomerIdInTimeFrame(customerId, startDate, endDate);
      if (customerReservationsInTimeFrameResult.IsFailure)
        return Result<bool>.Failure(customerReservationsInTimeFrameResult.Error);

      var overlappingCustomerReservations = customerReservationsInTimeFrameResult.Value
       .Where(r => currentRentalId == null || r.ID != currentRentalId);

      if (overlappingCustomerReservations.Any())
        return Result<bool>.Failure(Error.ValidationError("Requested reservation overlaps for this user!"));

      var vehicleReservationsInTimeFrameResult = await _rentalRepository.GetByVinInTimeFrame(vehicleId, startDate, endDate);
      if (vehicleReservationsInTimeFrameResult.IsFailure)
        return Result<bool>.Failure(vehicleReservationsInTimeFrameResult.Error);

      var overlappingVehicleReservations = vehicleReservationsInTimeFrameResult.Value
         .Where(r => currentRentalId == null || r.ID != currentRentalId);

      if (overlappingVehicleReservations.Any())
        return Result<bool>.Failure(Error.ValidationError("Requested reservation overlaps for this vehicle!"));

      return Result<bool>.Success(true);
    }


  }
}
