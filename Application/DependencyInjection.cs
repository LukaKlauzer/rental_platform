using System.Reflection;
using Application.DTOs.Customer;
using Application.DTOs.Rental;
using Application.DTOs.Vehicle;
using Application.Features.Customer.Commands.CreateCustomer;
using Application.Features.Customer.Commands.DeleteCustomer;
using Application.Features.Customer.Commands.UpdateCustomer;
using Application.Features.Customer.Queries.GetAllCustomers;
using Application.Features.Customer.Queries.GetCustomerById;
using Application.Features.Customer.Queries.LoginCustomer;
using Application.Features.Rentals.Commands.CancelReservation;
using Application.Features.Rentals.Commands.CreateReservation;
using Application.Features.Rentals.Commands.UpdateReservation;
using Application.Features.Rentals.Queries.GetAllReservations;
using Application.Features.Rentals.Queries.GetReservationById;
using Application.Features.Vehicles.Queries.GetAllVehicles;
using Application.Features.Vehicles.Queries.GetVehiclesByVin;
using Application.Interfaces.DataValidation;
using Application.Interfaces.Mapers;
using Application.Interfaces.Services;
using Application.Mapers;
using Application.Services;
using Application.ValidationBehavior;
using Core.Result;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Application
{
  public static class DependencyInjection
  {
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {

      // Register FluentValidation
      services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
      services.AddMediatR(cfg =>
      {
        cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
      });


      // Customer commands and queries
      services.AddTransient<IPipelineBehavior<LoginCustomerQuery, Result<string>>, ValidationBehavior<LoginCustomerQuery, string>>();
      services.AddTransient<IPipelineBehavior<CreateCustomerCommand, Result<CustomerReturnDto>>, ValidationBehavior<CreateCustomerCommand, CustomerReturnDto>>();
      services.AddTransient<IPipelineBehavior<UpdateCustomerCommand, Result<CustomerReturnDto>>, ValidationBehavior<UpdateCustomerCommand, CustomerReturnDto>>();
      services.AddTransient<IPipelineBehavior<DeleteCustomerCommand, Result<bool>>, ValidationBehavior<DeleteCustomerCommand, bool>>();
      services.AddTransient<IPipelineBehavior<GetAllCustomersQuery, Result<List<CustomerReturnDto>>>, ValidationBehavior<GetAllCustomersQuery, List<CustomerReturnDto>>>();
      services.AddTransient<IPipelineBehavior<GetCustomerByIdQuery, Result<CustomerReturnSingleDto>>, ValidationBehavior<GetCustomerByIdQuery, CustomerReturnSingleDto>>();

      // Rental commands and queries
      services.AddTransient<IPipelineBehavior<CreateReservationCommand, Result<RentalReturnDto>>, ValidationBehavior<CreateReservationCommand, RentalReturnDto>>();
      services.AddTransient<IPipelineBehavior<UpdateReservationCommand, Result<bool>>, ValidationBehavior<UpdateReservationCommand, bool>>();
      services.AddTransient<IPipelineBehavior<CancelReservationCommand, Result<bool>>, ValidationBehavior<CancelReservationCommand, bool>>();
      services.AddTransient<IPipelineBehavior<GetAllReservationsQuery, Result<List<RentalReturnDto>>>, ValidationBehavior<GetAllReservationsQuery, List<RentalReturnDto>>>();
      services.AddTransient<IPipelineBehavior<GetReservationByIdQuery, Result<RentalReturnSingleDto>>, ValidationBehavior<GetReservationByIdQuery, RentalReturnSingleDto>>();

      // Vehicle queries
      services.AddTransient<IPipelineBehavior<GetAllVehiclesQuery, Result<List<VehicleReturnDto>>>, ValidationBehavior<GetAllVehiclesQuery, List<VehicleReturnDto>>>();
      services.AddTransient<IPipelineBehavior<GetVehicleByVinQuery, Result<VehicleReturnSingleDto>>, ValidationBehavior<GetVehicleByVinQuery, VehicleReturnSingleDto>>();

      // Register MediatR


      // Services
      services.AddScoped<IRentalService, RentalService>();
      services.AddScoped<IVehicleService, VehicleService>();

      // Mappers
      services.AddScoped<ICustomerMapper, CustomerMapper>();
      services.AddScoped<IVehicleMapper, VehicleMapper>();
      services.AddScoped<IRentalMapper, RentalMapper>();

      // Validators
      services.AddScoped<ICustomerValidator, CustomerDtoValidator>();
      services.AddScoped<IRentalValidator, RentalDtoValidator>();
      services.AddScoped<IVehicleValidator, VehicleDtoValidator>();

      return services;
    }

  }
}
