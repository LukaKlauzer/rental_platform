using Core.Domain.Entities;
using Core.DTOs.Rental;
using Core.Enums;

namespace Core.Extensions
{
  public static class RentalExtensions
  {
    public static Rental? ToRental(this RentalCreateDTO rentalCreateDTO)
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
    public static RentalReturnDTO? ToReturnDto(this Rental rental)
    {
      if (rental is null) return null;

      return new RentalReturnDTO()
      {
        Id = rental.ID,
        StartDate = rental.StartDate,
        EndDate = rental.EndDate,
        RentalStatus = rental.RentalStatus,
        VehicleId = rental.VehicleId,
        CustomerId = rental.CustomerId,
      };
    }
    public static RentalReturnSingleDTO? ToReturnSingleDto(this Rental rental)
    {
      if (rental is null) return null;

      return new RentalReturnSingleDTO()
      {
        Id = rental.ID,
        StartDate = rental.StartDate,
        EndDate = rental.EndDate,
        RentalStatus = rental.RentalStatus,
        VehicleId = rental.VehicleId,
        CustomerId = rental.CustomerId,
      };
    }
    public static List<RentalReturnDTO> ToListRentalDto(this List<Rental> rental)
    {
      if (rental is null) return new List<RentalReturnDTO>();
      return rental.Select(r => new RentalReturnDTO()
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
