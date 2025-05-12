using Application.DTOs.Rental;
using Application.Interfaces.Mapers;
using Application.Interfaces.Persistence.SpecificRepository;
using Core.Result;
using MediatR;

namespace Application.Features.Rentals.Queries.GetAllReservations
{
  public class GetAllReservationsQueryHandler : IRequestHandler<GetAllReservationsQuery, Result<List<RentalReturnDto>>>
  {
    private readonly IRentalRepository _rentalRepository;
    private readonly IRentalMapper _rentalMapper;

    public GetAllReservationsQueryHandler(
        IRentalRepository rentalRepository,
        IRentalMapper rentalMapper)
    {
      _rentalRepository = rentalRepository;
      _rentalMapper = rentalMapper;
    }

    public async Task<Result<List<RentalReturnDto>>> Handle(GetAllReservationsQuery request, CancellationToken cancellationToken)
    {
      var allRentalsResult = await _rentalRepository.GetAll(cancellationToken);

      return allRentalsResult.Match(
          rentals => _rentalMapper.ToReturnDtoList(rentals),
          error => Result<List<RentalReturnDto>>.Failure(error)
      );
    }
  }
}
