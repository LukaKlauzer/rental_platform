using Application.DTOs.Customer;
using Application.Interfaces.Authentification;
using Application.Interfaces.DataValidation;
using Application.Interfaces.Mapers;
using Application.Interfaces.Persistence.SpecificRepository;
using Application.Services;
using Core.Domain.Entities;
using Core.Result;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using CustomerEntity = Core.Domain.Entities.Customer;

namespace RentalPlatform.Tests.Unit.Application.Customer
{
  public class CustomerServiceTests
  {
    private readonly Mock<ICustomerRepository> _mockCustomerRepository;
    private readonly Mock<IRentalRepository> _mockRentalRepository;
    private readonly Mock<IVehicleRepository> _mockVehicleRepository;
    private readonly Mock<ICustomerMapper> _mockCustomerMapper;
    private readonly Mock<IJwtTokenGenerator> _mockJwtTokenGenerator;
    private readonly Mock<ICustomerValidator> _mockCustomerValidator;
    private readonly CustomerService _customerService;

    public CustomerServiceTests()
    {
      ILogger<CustomerService> logger = NullLogger<CustomerService>.Instance;

      _mockCustomerRepository = new Mock<ICustomerRepository>();
      _mockRentalRepository = new Mock<IRentalRepository>();
      _mockVehicleRepository = new Mock<IVehicleRepository>();
      _mockCustomerMapper = new Mock<ICustomerMapper>();
      _mockJwtTokenGenerator = new Mock<IJwtTokenGenerator>();
      _mockCustomerValidator = new Mock<ICustomerValidator>();

      _customerService = new CustomerService(
          logger,
          _mockCustomerRepository.Object,
          _mockRentalRepository.Object,
          _mockVehicleRepository.Object,
          _mockCustomerMapper.Object,
          _mockJwtTokenGenerator.Object,
          _mockCustomerValidator.Object);
    }

    [Fact]
    public async Task Create_WithValidData_ShouldReturnSuccess()
    {
      // Arrange
      var createCustomerDto = new CustomerCreateDto("Test customer 1");
      var customer = CustomerEntity.Create("Test customer 1").Value;
      var returnDto = new CustomerReturnDto(1, "Test customer 1", false);

      _mockCustomerValidator.Setup(v => v.ValidateCreate(It.IsAny<CustomerCreateDto>()))
        .Returns(Result<bool>.Success(true));

      _mockCustomerMapper.Setup(m => m.ToEntity(It.IsAny<CustomerCreateDto>()))
        .Returns(Result<CustomerEntity>.Success(customer));

      _mockCustomerRepository.Setup(repo => repo
        .Create(It.IsAny<CustomerEntity>(), default))
        .ReturnsAsync(Result<CustomerEntity>.Success(customer));

      _mockCustomerMapper.Setup(m => m.ToReturnDto(It.IsAny<CustomerEntity>()))
        .Returns(Result<CustomerReturnDto>.Success(returnDto));

      // Act
      var result = await _customerService.Create(createCustomerDto);

      // Assert
      Assert.True(result.IsSuccess);
      Assert.Equal("Test customer 1", result.Value.Name);
    }

    [Fact]
    public async Task CreateWithInvalidData_ShouldReturnError()
    {
      // Arrange 
      var createCustomerDto = new CustomerCreateDto("");

      _mockCustomerValidator.Setup(v => v.ValidateCreate(It.IsAny<CustomerCreateDto>()))
        .Returns(Result<bool>.Failure(Error.ValidationError("Customer name can not be empty")));

      // Act
      var result = await _customerService.Create(createCustomerDto);

      // Assert
      Assert.True(result.IsFailure);
      Assert.Equal(ErrorType.ValidationError, result.Error.ErrorType);
    }

    [Fact]
    public async Task CreateWithNullDto_ShouldReturnError()
    {
      // Arrange 
      CustomerCreateDto createCustomerDto = null!;

      _mockCustomerValidator.Setup(v => v.ValidateCreate(It.IsAny<CustomerCreateDto>()))
        .Returns(Result<bool>.Failure(Error.NullReferenceError("CustomerReturnDto cannot be null.")));

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
        .ReturnsAsync(Result<CustomerEntity>.Failure(Error.NotFound($"Customer with id {customerId} not found")));

      // Act
      var result = await _customerService.GetById(customerId);

      // Assert
      Assert.True(result.IsFailure);
      Assert.Equal(ErrorType.NotFound, result.Error.ErrorType);
    }

    [Fact]
    public async Task GetById_CustomerWithCompletedRentals_CalculatesTotalsCorrectly()
    {
      // Arrange
      var customerId = 1;
      string vehicle1Vin = "WAUZZZ4V4KA000002";

      var customer = CustomerEntity.Create("Test customer 1").Value;
      var customerSingleDto = new CustomerReturnSingleDto(1, "Test customer 1", false, 500f, 510f);

      var rentals = new List<Rental> {
        Rental.Create(
          startDate: new DateTime(2025, 1, 1),
          endDate: new DateTime(2025, 1, 3),
          odometerStart: 0,
          odometerEnd: 500,
          batterySOCStart: 80,
          batterySOCEnd: 30,
          vehicleId: vehicle1Vin,
          customerId: customerId
        ).Value
      };

      var vehicleVins = new List<string> { vehicle1Vin };
      var vehicles = new List<Vehicle>
      {
        Vehicle.Create(
          vin: vehicle1Vin,
          make: "Mazda",
          model: "Miata",
          year: 2019,
          pricePerKmInEuro: 0.5f,
          pricePerDayInEuro: 100f
        ).Value
      };

      _mockCustomerRepository.Setup(repo => repo.GetById(customerId, default))
        .ReturnsAsync(Result<CustomerEntity>.Success(customer));

      _mockRentalRepository.Setup(repo => repo.GetByCustomerId(customerId, default))
        .ReturnsAsync(Result<IEnumerable<Rental>>.Success(rentals));

      _mockVehicleRepository.Setup(repo => repo.GetByVins(vehicleVins, default))
        .ReturnsAsync(Result<IEnumerable<Vehicle>>.Success(vehicles));

      _mockCustomerMapper.Setup(m => m.ToReturnSingleDto(It.IsAny<CustomerEntity>(), It.IsAny<float>(), It.IsAny<float>()))
        .Returns((CustomerEntity c, float distance, float price) =>
          Result<CustomerReturnSingleDto>.Success(new CustomerReturnSingleDto(c.ID, c.Name, c.IsDeleted, distance, price)));

      // Act
      var result = await _customerService.GetById(customerId);

      // Assert
      Assert.True(result.IsSuccess);
      Assert.Equal(500, result.Value.TotalDistanceDriven);
      Assert.Equal(460, result.Value.TotalPrice); // 500km * 0.5€/km + 2 days * 100€/day + 50 * 0.2€ penalty => 460
    }
  }
}