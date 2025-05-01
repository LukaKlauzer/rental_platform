using Core.DTOs.Rental;
using Core.Features.Rental.Queries;
using Core.Interfaces.Persistence.SpecificRepository;
using Core.Result;
using MediatR;
using Microsoft.Extensions.Logging;
using Core.Extensions;

namespace Application.Features.Rental.Queries
{
  public class GetByIdRentalQueryHandler : IRequestHandler<GetByIdRentalQuery, Result<RentalReturnSingleDTO>>
  {
    private readonly ILogger<GetByIdRentalQueryHandler> _logger;
    private readonly IRentalRepository _rentalRepository;

    public GetByIdRentalQueryHandler(
        ILogger<GetByIdRentalQueryHandler> logger,
        IRentalRepository rentalRepository)
    {
      _logger = logger;
      _rentalRepository = rentalRepository;
    }

    public async Task<Result<RentalReturnSingleDTO>> Handle(GetByIdRentalQuery request, CancellationToken cancellationToken)
    {
      var rentalResult = await _rentalRepository.GetById(request.Id, cancellationToken);
      if (rentalResult.IsFailure)
      {
        _logger.LogWarning("Failed to retrieve rental with ID {RentalId}: {ErrorMessage}",
            request.Id, rentalResult.Error.Message);
        return Result<RentalReturnSingleDTO>.Failure(rentalResult.Error);
      }

      var returnDto = rentalResult.Value.ToReturnSingleDto();
      if (returnDto is null)
      {
        _logger.LogError("Failed to map rental entity to rental DTO for rental ID {RentalId}", request.Id);
        return Result<RentalReturnSingleDTO>.Failure(Error.MappingError("Failed to map rental entity to rental DTO"));
      }

      if (rentalResult.Value.OdometerEnd.HasValue)
        returnDto.DistanceTraveled = rentalResult.Value.OdometerEnd.Value - rentalResult.Value.OdometerStart;

      returnDto.BatterySOCSAtStart = rentalResult.Value.BatterySOCStart;

      if (rentalResult.Value.BatterySOCEnd.HasValue)
        returnDto.BatterySOCAtEnd = rentalResult.Value.BatterySOCEnd.Value;

      _logger.LogInformation("Successfully retrieved rental details for rental ID {RentalId}", request.Id);
      return Result<RentalReturnSingleDTO>.Success(returnDto);
    }

  }
}
