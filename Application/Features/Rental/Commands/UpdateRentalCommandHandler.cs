using Core.Features.Rental.Commands;
using Core.Interfaces.Persistence.SpecificRepository;
using Core.Result;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Rental.Commands
{
  public class UpdateRentalCommandHandler : IRequestHandler<UpdateRentalCommand, Result<bool>>
  {
    private readonly ILogger<UpdateRentalCommandHandler> _logger;
    private readonly IRentalRepository _rentalRepository;

    public UpdateRentalCommandHandler(
        ILogger<UpdateRentalCommandHandler> logger,
        IRentalRepository rentalRepository)
    {
      _logger = logger;
      _rentalRepository = rentalRepository;
    }

    public async Task<Result<bool>> Handle(UpdateRentalCommand request, CancellationToken cancellationToken)
    {
      if (request.Id <= 0)
      {
        _logger.LogWarning("Rental update validation failed: Invalid ID ({RentalId})", request.Id);
        return Result<bool>.Failure(Error.ValidationError($"Rental id is not valid: {request.Id}"));
      }

      if (!request.StartDate.HasValue && !request.EndDate.HasValue)
      {
        _logger.LogWarning("Rental update validation failed: No update fields provided for rental {RentalId}", request.Id);
        return Result<bool>.Failure(Error.ValidationError("At least one field must be provided for update"));
      }

      var rentalResult = await _rentalRepository.GetById(request.Id, cancellationToken);
      if (rentalResult.IsFailure)
        return Result<bool>.Failure(rentalResult.Error);

      // Check for overlapping reservations
      var startDate = request.StartDate ?? rentalResult.Value.StartDate;
      var endDate = request.EndDate ?? rentalResult.Value.EndDate;

      // Get customer and vehicle IDs from the existing rental
      var customerId = rentalResult.Value.CustomerId;
      var vehicleId = rentalResult.Value.VehicleId;

      // Check for overlapping reservations
      var customerReservationsInTimeFrameResult = await _rentalRepository.GetByCustomerIdInTimeFrame(
          customerId, startDate, endDate, cancellationToken);

      if (customerReservationsInTimeFrameResult.IsFailure)
        return Result<bool>.Failure(customerReservationsInTimeFrameResult.Error);

      var overlappingCustomerReservations = customerReservationsInTimeFrameResult.Value
          .Where(r => r.ID != request.Id);

      if (overlappingCustomerReservations.Any())
      {
        _logger.LogWarning("Overlapping reservation detected when updating rental {RentalId} for " +
            "customer {CustomerId}, dates {StartDate:d} to {EndDate:d}",
            request.Id, customerId, startDate, endDate);
        return Result<bool>.Failure(Error.ValidationError("Requested reservation overlaps for this user!"));
      }

      var vehicleReservationsInTimeFrameResult = await _rentalRepository.GetByVinInTimeFrame(
          vehicleId, startDate, endDate, cancellationToken);

      if (vehicleReservationsInTimeFrameResult.IsFailure)
        return Result<bool>.Failure(vehicleReservationsInTimeFrameResult.Error);

      var overlappingVehicleReservations = vehicleReservationsInTimeFrameResult.Value
          .Where(r => r.ID != request.Id);

      if (overlappingVehicleReservations.Any())
      {
        _logger.LogWarning("Overlapping reservation detected when updating rental {RentalId} for " +
            "vehicle {VehicleId}, dates {StartDate:d} to {EndDate:d}",
            request.Id, vehicleId, startDate, endDate);
        return Result<bool>.Failure(Error.ValidationError("Requested reservation overlaps for this vehicle!"));
      }

      // Update the rental dates
      if (request.StartDate.HasValue)
        rentalResult.Value.StartDate = request.StartDate.Value;

      if (request.EndDate.HasValue)
        rentalResult.Value.EndDate = request.EndDate.Value;

      var updatedRental = await _rentalRepository.Update(rentalResult.Value, cancellationToken);

      return updatedRental.Match(
          rental =>
          {
            _logger.LogInformation("Successfully updated rental {RentalId} for customer {CustomerId}, " +
                      "vehicle {VehicleId}, new dates {StartDate:d} to {EndDate:d}",
                      rental.ID, rental.CustomerId, rental.VehicleId, rental.StartDate, rental.EndDate);
            return Result<bool>.Success(true);
          },
          error =>
          {
            _logger.LogError("Failed to update rental {RentalId}: {ErrorMessage}",
                      request.Id, error.Message);
            return Result<bool>.Failure(error);
          }
      );
    }
  }
}
