using Core.Enums;
using Core.Features.Rental.Commands;
using Core.Interfaces.Persistence.SpecificRepository;
using Core.Result;
using MediatR;

namespace Application.Features.Rental.Commands
{
  public class CancleRentalCommandHandler : IRequestHandler<CancelRentalCommand, Result<bool>>
  {
    private readonly IRentalRepository _rentalRepository;
    public CancleRentalCommandHandler(IRentalRepository rentalRepository)
    {
      _rentalRepository = rentalRepository;
    }

    public async Task<Result<bool>> Handle(CancelRentalCommand request, CancellationToken cancellationToken)
    {
      var rentalResult = await _rentalRepository.GetById(request.Id);
      if (rentalResult.IsFailure)
        return Result<bool>.Failure(rentalResult.Error);
      rentalResult.Value.RentalStatus = RentalStatus.Cancelled;

      var updatedRentalResult = await _rentalRepository.Update(rentalResult.Value);

      return updatedRentalResult.Match(
        rental => Result<bool>.Success(true),
        error => Result<bool>.Failure(error)
        );
    }
  }
}
