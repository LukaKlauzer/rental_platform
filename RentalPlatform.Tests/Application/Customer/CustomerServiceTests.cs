using Application.Services;
using Core.Domain.Entities;
using Application.DTOs.Customer;
using Core.Enums;
using Application.Interfaces.Persistence.SpecificRepository;
using Application.Interfaces.Services;
using Core.Result;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging;
using Moq;
using CustomerEntity = Core.Domain.Entities.Customer;

namespace RentalPlatform.UnitTests.Application.Customer
{
  public class CustomerServiceTests
  {
    private readonly Mock<ICustomerRepository> _mockCustomerRepository;
    private readonly Mock<IRentalRepository> _mockRentalRepository;
    private readonly Mock<IVehicleRepository> _mockVehicleRepository;
    private readonly Mock<IJwtTokenGenerator> _mockJwtTokenGenerator;
    private readonly ICustomerService _customerService;

    public CustomerServiceTests()
    {
      ILogger<CustomerSevice> logger = NullLogger<CustomerSevice>.Instance;

      _mockCustomerRepository = new Mock<ICustomerRepository>();
      _mockRentalRepository = new Mock<IRentalRepository>();
      _mockVehicleRepository = new Mock<IVehicleRepository>();
      _mockJwtTokenGenerator = new Mock<IJwtTokenGenerator>();

      _customerService = new CustomerSevice(
          logger,
          _mockCustomerRepository.Object,
          _mockRentalRepository.Object,
          _mockVehicleRepository.Object,
          _mockJwtTokenGenerator.Object);
    }
    [Fact]
    public async Task Create_WithValidData_ShouldReturnSuccess()
    {
      // Arange
      var createCustomerDto = new CustomerCreateDto("Test customer 1");
      var customer = CustomerEntity.Create("Test Customer 1");

      _mockCustomerRepository.Setup(repo => repo
        .Create(It.IsAny<CustomerEntity>(), default))
        .ReturnsAsync(Result<CustomerEntity>.Success(customer.Value));

      // Act
      var result = await _customerService.Create(createCustomerDto);

      // Assert
      Assert.True(result.IsSuccess);
      Assert.Equal("Test Customer 1", result.Value.Name);
    }


    [Fact]
    public async Task CreateWithNotValidData_ShouldReturnError()
    {
      // Arange 
      var createCustomerDto = new CustomerCreateDto("");

      // Act
      var result = await _customerService.Create(createCustomerDto);

      // Assert
      Assert.True(result.IsFailure);
      Assert.Equal(ErrorType.ValidationError, result.Error.ErrorType);
    }

    [Fact]
    public async Task CreateWithNullDto_ShouldReturnError()
    {
      // Arange 
      CustomerCreateDto createCustomerDto = null;

      // Act
      var result = await _customerService.Create(createCustomerDto);

      // Assert
      Assert.True(result.IsFailure);
      Assert.Equal(ErrorType.NullReference, result.Error.ErrorType);
    }

    [Fact]
    public async Task GetById_CustomerNotFound_ReturnsError()
    {
      // Arrange
      var customerId = 1;
      _mockCustomerRepository.Setup(repo => repo.GetById(customerId, default))
        .ReturnsAsync(Result<CustomerEntity>.Failure(Error.NotFound("Customer with id '{customerId}'")));

      // Act
      var result = await _customerService.GetById(customerId);

      // Assert
      Assert.True(result.IsFailure);
      Assert.Equal(ErrorType.NotFound, result.Error.ErrorType);
    }

    [Fact]
    public async Task GetById_CustomerWithCompleatedRentals_CalculatesTortalsCorrectly()
    {
      // Arrange
      var customerId = 1;
      string vehicle1Vin = "VIN2";
      string vehicle2Vin = "VIN1";

      var customer = CustomerEntity.Create("Test customer 1").Value;

      var rentals = new List<Rental> {

        Rental.Create
        (
          customerId: customerId,
          vehicleId: vehicle1Vin,

          startDate: new DateTime(2025, 1, 1),
          endDate: new DateTime(2025, 1, 3),

          odometerStart: 0,
          odometerEnd: 500,

          batterySOCStart: 80,
          batterySOCEnd: 30
        ).Value
      };

      var vehacleVins = new List<string> { vehicle1Vin };
      var vehicles = new List<Vehicle>
      {
        Vehicle.Create
       (
          vin : vehicle1Vin,
          make : "Mazda",
          model : "Miata",
          year : 2019,
          pricePerDayInEuro : 0.5f,
          pricePerKmInEuro : 100f
        ).Value
    };
      var vehicleDict = vehicles.ToDictionary(v => v.Vin);

      _mockCustomerRepository.Setup(repo => repo.GetById(customerId, default)).ReturnsAsync(Result<CustomerEntity>.Success(customer));
      _mockRentalRepository.Setup(repo => repo.GetByCustomerId(customerId, default)).ReturnsAsync(Result<IEnumerable<Rental>>.Success(rentals));
      _mockVehicleRepository.Setup(repo => repo.GetByVins(vehacleVins, default)).ReturnsAsync(Result<IEnumerable<Vehicle>>.Success(vehicles));

      // Act
      var result = await _customerService.GetById(customerId);

      /**
        `total_kilometers_per_rental × price_per_km_in_euro`  
        `+ number_of_rental_days × price_per_day_in_euro`  
        `+ max(0, -battery_delta_per_rental) × 0.2€`
      **/
      var rentalStats = rentals.Select(rental =>
      {
        if (!vehicleDict.TryGetValue(rental.VehicleId, out var vehicle))
          return null;
        float distance = rental.OdometerEnd!.Value - rental.OdometerStart;
        float days = Math.Max(1, (int)Math.Ceiling((rental.EndDate - rental.StartDate).TotalDays));
        float batteryDelta = rental.BatterySOCEnd!.Value - rental.BatterySOCStart;
        return new
        {
          Distance = distance,
          Days = days,
          Cost = distance * vehicle.PricePerKmInEuro + days * vehicle.PricePerDayInEuro + Math.Max(0, -batteryDelta) * 0.2f,
          Vehicle = vehicle
        };
      }

        );
      var distance = rentalStats.Sum(s => s!.Distance);
      var price = rentalStats.Sum(s => s!.Cost);

      // Assert
      Assert.True(result.IsSuccess);
      Assert.Equal(distance, result.Value.TotalDistanceDriven);
      Assert.Equal(price, result.Value.TotalPrice);
    }
  }
}
