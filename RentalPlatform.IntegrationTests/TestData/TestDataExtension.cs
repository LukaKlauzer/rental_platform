using Core.Domain.Entities;
using Core.Enums;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace RentalPlatform.Tests.Integration.TestData
{
  public static class TestDataExtension
  {
    /// <summary>
    /// Extension method for CustomWebApplicationFactory to seed data needed for tests
    /// </summary>
    public static async Task<TestData> SeedTestDataAsync(
      this CustomWebApplicationFactory<Program> factory)
    {
      var testContext = new TestData();

      using var scope = factory.Services.CreateScope();
      var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

      // Create test customers 
      var firstCustomer = Customer.Create("Test customer 1").Value;
      var secondCustomer = Customer.Create("Test customer 2").Value;

      await dbContext.Customers.AddAsync(firstCustomer);
      await dbContext.Customers.AddAsync(secondCustomer);

      await dbContext.SaveChangesAsync();
      testContext.FirstCustomerId = firstCustomer.ID;
      testContext.SecondCustomerId = secondCustomer.ID;

      // Create test vehicle
      var vehicle = Vehicle.Create(
        vin: "WAUZZZ4V4KA000002",
        make: "Test Make",
        model: "Test Model",
        year: 2023,
        pricePerKmInEuro: 0.5f,
        pricePerDayInEuro: 50.0f
      ).Value;

      await dbContext.Vehicles.AddAsync(vehicle);
      await dbContext.SaveChangesAsync();
      testContext.VehicleVin = vehicle.Vin;

      // Add telemetry data for the vehicle
      var now = DateTime.UtcNow;
      var odometerTelemetry = new List<Telemetry>();

      // Create odometer telemetry entries
      var odometerData = new[]
      {
        (1000.0f, 1735689600), // 2025-01-01 00:00:00
        (1010.0f, 1735711200), // 2025-01-01 06:00:00
        (1020.0f, 1735732800), // 2025-01-01 12:00:00
        (1030.0f, 1735754400), // 2025-01-01 18:00:00
        (1040.0f, 1735776000), // 2025-01-02 00:00:00
        (1050.0f, 1735819200), // 2025-01-02 12:00:00
        (1060.0f, 1735862400), // 2025-01-03 00:00:00
        (1070.0f, 1735905600), // 2025-01-03 12:00:00
        (1080.0f, 1735948800), // 2025-01-04 00:00:00
        (1090.0f, 1735992000), // 2025-01-04 12:00:00
        (1100.0f, 1736035200), // 2025-01-05 00:00:00
        (1110.0f, 1736078400)  // 2025-01-05 12:00:00
      };

      foreach (var (value, timestamp) in odometerData)
      {
        var telemetry = Telemetry.Create(
          telemetryType: TelemetryType.odometer,
          value: value,
          timestamp: timestamp,
          vehicleId: vehicle.Vin
        ).Value;
        odometerTelemetry.Add(telemetry);
      }

      var batteryTelemetry = new List<Telemetry>();

      // Create battery SOC telemetry entries
      var batteryData = new[]
      {
        (87.6f, 1735689600),  // 2025-01-01 00:00:00
        (83.2f, 1735711200),  // 2025-01-01 06:00:00
        (79.1f, 1735732800),  // 2025-01-01 12:00:00
        (70.3f, 1735754400),  // 2025-01-01 18:00:00
        (58.0f, 1735776000),  // 2025-01-02 00:00:00
        (45.7f, 1735797600),  // 2025-01-02 06:00:00
        (31.5f, 1735819200),  // 2025-01-02 12:00:00
        (18.2f, 1735840800),  // 2025-01-02 18:00:00
        (92.8f, 1735862400),  // 2025-01-03 00:00:00
        (86.5f, 1735905600),  // 2025-01-03 12:00:00
        (74.2f, 1735948800),  // 2025-01-04 00:00:00
        (61.7f, 1735992000)   // 2025-01-04 12:00:00
      };

      foreach (var (value, timestamp) in batteryData)
      {
        var telemetry = Telemetry.Create(
          telemetryType: TelemetryType.battery_soc,
          value: value,
          timestamp: timestamp,
          vehicleId: vehicle.Vin
        ).Value;
        batteryTelemetry.Add(telemetry);
      }

      await dbContext.Telemetries.AddRangeAsync(odometerTelemetry);
      await dbContext.Telemetries.AddRangeAsync(batteryTelemetry);
      await dbContext.SaveChangesAsync();

      return testContext;
    }

    /// <summary>
    /// Extension method for CustomWebApplicationFactory to clean up test data
    /// </summary>
    public static async Task CleanupTestDataAsync(
      this CustomWebApplicationFactory<Program> factory,
      TestData testContext)
    {
      using var scope = factory.Services.CreateScope();
      var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

      // Remove rentals
      var rentals = await dbContext.Rentals
          .Where(r => r.VehicleId == testContext.VehicleVin ||
                      r.CustomerId == testContext.FirstCustomerId ||
                      r.CustomerId == testContext.SecondCustomerId)
          .ToListAsync();

      dbContext.Rentals.RemoveRange(rentals);

      // Remove telemetry
      var telemetry = await dbContext.Telemetries
          .Where(t => t.VehicleId == testContext.VehicleVin)
          .ToListAsync();

      dbContext.Telemetries.RemoveRange(telemetry);

      // Remove vehicle
      var vehicle = await dbContext.Vehicles.FindAsync(testContext.VehicleVin);
      if (vehicle != null)
        dbContext.Vehicles.Remove(vehicle);

      // Remove customers
      var customer1 = await dbContext.Customers.FindAsync(testContext.FirstCustomerId);
      if (customer1 != null)
        dbContext.Customers.Remove(customer1);

      var customer2 = await dbContext.Customers.FindAsync(testContext.SecondCustomerId);
      if (customer2 != null)
        dbContext.Customers.Remove(customer2);

      await dbContext.SaveChangesAsync();
    }
  }

  public class TestData
  {
    public int FirstCustomerId { get; set; }
    public int SecondCustomerId { get; set; }
    public string VehicleVin { get; set; } = string.Empty;
  }
}