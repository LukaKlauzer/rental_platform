using Application.Interfaces.Persistence.SpecificRepository;
using Core.Result;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Rentals.Commands.CancelReservation
{
  public class CancelReservationCommandHandler : IRequestHandler<CancelReservationCommand, Result<bool>>
  {
    private readonly ILogger<CancelReservationCommandHandler> _logger;
    private readonly IRentalRepository _rentalRepository;

    public CancelReservationCommandHandler(
        ILogger<CancelReservationCommandHandler> logger,
        IRentalRepository rentalRepository)
    {
      _logger = logger;
      _rentalRepository = rentalRepository;
    }

    public async Task<Result<bool>> Handle(CancelReservationCommand command, CancellationToken cancellationToken)
    {
      var rentalResult = await _rentalRepository.GetById(command.Id, cancellationToken);
      if (rentalResult.IsFailure)
        return Result<bool>.Failure(rentalResult.Error);

      var canceledResult = rentalResult.Value.Cancel();
      if (canceledResult.IsFailure)
        return canceledResult;

      var updatedRentalResult = await _rentalRepository.Update(rentalResult.Value, cancellationToken);

      return updatedRentalResult.Match(
          rental => Result<bool>.Success(true),
          error => Result<bool>.Failure(error)
      );
    }
  }
}
