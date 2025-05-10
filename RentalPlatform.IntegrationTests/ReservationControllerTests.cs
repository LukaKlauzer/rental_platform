using Core.DTOs.Customer;
using System.Net.Http.Json;
using RentalPlatform.Tests.Integration;
using FluentAssertions;
using Core.DTOs.Rental;
using Core.Enums;
using RentalPlatform.Tests.Integration.TestData;

namespace RentalPlatform.Tests.Unit.Application.Reservation
{
  public class ReservationControllerTests :
    IClassFixture<CustomWebApplicationFactory<Program>>,
    IAsyncLifetime
  {
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly HttpClient _httpClient;
    private TestData _testData = null!;
    public ReservationControllerTests(CustomWebApplicationFactory<Program> factory)
    {
      _factory = factory;
      _httpClient = _factory.CreateClient();
    }
    [Fact]
    public async Task CancelReservation_ShouldUpdateRentalStatus()
    {
      // Arrange
      var rentalCreateDto = new RentalDTO
      {
        CustomerId = _testData.FirstCustomerId,
        VehicleId = _testData.VehicleVin,
        StartDate = new DateTime(2025, 1, 2),
        EndDate = new DateTime(2025, 1, 3)
      };

      // Create the rental
      var createResponse = await _httpClient.PostAsJsonAsync("api/rental", rentalCreateDto);
      var createdRental = await createResponse.Content.ReadFromJsonAsync<RentalReturnDto>();
      createdRental.Should().NotBeNull();

      // Act - Cancel the rental
      var cancelResponse = await _httpClient.PutAsync($"api/rental/{createdRental!.Id}", null);

      // Assert
      cancelResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

      // Verify the rental status was updated
      var getResponse = await _httpClient.GetAsync($"api/rental/{createdRental.Id}");
      var retrievedRental = await getResponse.Content.ReadFromJsonAsync<RentalReturnSingleDto>();
      retrievedRental.Should().NotBeNull();
      retrievedRental!.RentalStatus.Should().Be(RentalStatus.Cancelled);
    }
    [Fact]
    public async Task CreateReservation_WhenOverlappingExists_ReturnsConflictResult()
    {
      var rentalCreateDto = new RentalDTO
      {
        CustomerId = _testData.FirstCustomerId,
        VehicleId = _testData.VehicleVin,
        StartDate = DateTime.UtcNow.AddDays(1),
        EndDate = DateTime.UtcNow.AddDays(3)
      };
      var overlappingRentalCreateDto = new RentalDTO
      {
        CustomerId = _testData.SecondCustomerId,
        VehicleId = _testData.VehicleVin,
        StartDate = DateTime.UtcNow.AddDays(2),
        EndDate = DateTime.UtcNow.AddDays(3)
      };

      // Create the rental
      var createResponse = await _httpClient.PostAsJsonAsync("api/rental", rentalCreateDto);
      var createdRental = await createResponse.Content.ReadFromJsonAsync<RentalReturnDto>();
      createdRental.Should().NotBeNull();
      createdRental.RentalStatus.Should().Be(RentalStatus.Ordered);

      // Create overlapping  rental
      var createOverlappingResponse = await _httpClient.PostAsJsonAsync("api/rental", overlappingRentalCreateDto);

      createOverlappingResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

      var errorMessage = await createOverlappingResponse.Content.ReadAsStringAsync();
      errorMessage.Should().Contain("Requested reservation overlaps for this vehicle!");
    }

    public async Task InitializeAsync()
    {
      // Seed test data using our extension method
      _testData = await _factory.SeedTestDataAsync();
    }

    public async Task DisposeAsync()
    {
      // Clean up all test data
      await _factory.CleanupTestDataAsync(_testData);
    }

  }
}
