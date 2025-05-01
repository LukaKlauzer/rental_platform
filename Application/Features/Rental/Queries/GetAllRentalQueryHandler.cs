using Core.DTOs.Rental;
using Core.Features.Customer.Commands;
using Core.Interfaces.Persistence.SpecificRepository;
using Core.Result;
using Core.Extensions;
using MediatR;
using Core.Features.Rental.Queries;

namespace Application.Features.Rental.Queries
{
  public class GetAllRentalQueryHandler : IRequestHandler<GetAllRentalsQuery, Result<List<RentalReturnDTO>>>
  {
    private readonly IRentalRepository _rentalRepository;
    public GetAllRentalQueryHandler(IRentalRepository rentalRepository) 
    {
      _rentalRepository = rentalRepository;
    }
    public async Task<Result<List<RentalReturnDTO>>> Handle(GetAllRentalsQuery request, CancellationToken cancellationToken)
    {
      var allRentalsResult = await _rentalRepository.GetAll();

      return allRentalsResult.Match(
        rentals => Result<List<RentalReturnDTO>>.Success(rentals.ToList().ToListRentalDto()),
        error => Result<List<RentalReturnDTO>>.Failure(error)
        );
    }
  }
}
