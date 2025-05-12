using Application.DTOs.Rental;
using Application.Interfaces.Mapers;
using Application.Interfaces.Persistence.SpecificRepository;
using Core.Result;
using MediatR;

namespace Application.Features.Rentals.Queries.GetReservationById
{
  public class GetReservationByIdQueryHandler : IRequestHandler<GetReservationByIdQuery, Result<RentalReturnSingleDto>>
  {
    private readonly IRentalRepository _rentalRepository;
    private readonly IRentalMapper _rentalMapper;

    public GetReservationByIdQueryHandler(
        IRentalRepository rentalRepository,
        IRentalMapper rentalMapper)
    {
      _rentalRepository = rentalRepository;
      _rentalMapper = rentalMapper;
    }

    public async Task<Result<RentalReturnSingleDto>> Handle(GetReservationByIdQuery request, CancellationToken cancellationToken)
    {
      var rentalResult = await _rentalRepository.GetById(request.Id, cancellationToken);
      if (rentalResult.IsFailure)
        return Result<RentalReturnSingleDto>.Failure(rentalResult.Error);

      var rental = rentalResult.Value;

      var distanceResult = rental.GetDistanceTraveled();
      if (distanceResult.IsFailure)
        return Result<RentalReturnSingleDto>.Failure(distanceResult.Error);

      float? distanceTraveled = distanceResult.IsSuccess ? distanceResult.Value : null;

      return _rentalMapper.ToReturnSingleDto(rental, distanceTraveled);
    }
  }
}
