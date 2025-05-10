using Application.DTOs.Rental;
using Core.Domain.Entities;

namespace Application.Extensions
{
  public static class RentalExtensions
  {
    public static Rental? ToRental(this RentalCreateDto rentalCreateDTO)
    {
      if (rentalCreateDTO is null) return null;

      return new Rental()
      {
        StartDate = rentalCreateDTO.StartDate,
        EndDate = rentalCreateDTO.EndDate,
        RentalStatus = RentalStatus.Ordered,
        VehicleId = rentalCreateDTO.VehicleId,
        CustomerId = rentalCreateDTO.CustomerId,
      };
    }
    public static RentalReturnDto? ToReturnDto(this Rental rental)
    {
      if (rental is null) return null;

      return new RentalReturnDto()
      {
        Id = rental.ID,
        StartDate = rental.StartDate,
        EndDate = rental.EndDate,
        RentalStatus = rental.RentalStatus,
        VehicleId = rental.VehicleId,
        CustomerId = rental.CustomerId,
      };
    }
    public static RentalReturnSingleDto? ToReturnSingleDto(this Rental rental)
    {
      if (rental is null) return null;

      return new RentalReturnSingleDto()
      {
        Id = rental.ID,
        StartDate = rental.StartDate,
        EndDate = rental.EndDate,
        RentalStatus = rental.RentalStatus,
        VehicleId = rental.VehicleId,
        CustomerId = rental.CustomerId,
      };
    }
    public static List<RentalReturnDto> ToListRentalDto(this List<Rental> rental)
    {
      if (rental is null) return new List<RentalReturnDto>();
      return rental.Select(r => new RentalReturnDto()
      {
        Id = r.ID,
        StartDate = r.StartDate,
        EndDate = r.EndDate,
        RentalStatus = r.RentalStatus,
        VehicleId = r.VehicleId,
        CustomerId = r.CustomerId,
      }).ToList();
    }
  }
}
