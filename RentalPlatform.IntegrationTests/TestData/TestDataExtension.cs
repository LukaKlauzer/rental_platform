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

      // Create test customer
      var firstCustomer = new Customer
      {
        Name = $"Test customer 1",
        DateCreated = DateTime.UtcNow,
        DateModified = DateTime.UtcNow
      };
      var secondCustomer = new Customer
      {
        Name = $"Test customer 2",
        DateCreated = DateTime.UtcNow,
        DateModified = DateTime.UtcNow
      };

      await dbContext.Customers.AddAsync(firstCustomer);
      await dbContext.Customers.AddAsync(secondCustomer);
      
      await dbContext.SaveChangesAsync();
      testContext.FirstCustomerId = firstCustomer.ID;
      testContext.SecondCustomerId = secondCustomer.ID;

      // Create test vehicle
      var vehicle = new Vehicle
      {
        Vin = "VIN_123",
        Make = "Test Make",
        Model = "Test Model",
        Year = 2023,
        PricePerKmInEuro = 0.5f,
        PricePerDayInEuro = 50.0f,
        DateCreated = DateTime.UtcNow,
        DateModified = DateTime.UtcNow
      };

      await dbContext.Vehicles.AddAsync(vehicle);
      await dbContext.SaveChangesAsync();
      testContext.VehicleVin = vehicle.Vin;

      // Add telemetry data for the vehicle
      var now = DateTime.UtcNow;
      var timestamp = ((DateTimeOffset)now).ToUnixTimeSeconds();
      var odometerTelemetry = new List<Telemetry>(){
       new Telemetry
      {
        VehicleId = vehicle.Vin,
        Name = TelemetryType.odometer,
        Value = 1000.0f,
        Timestamp = 1735689600, // 2025-01-01 00:00:00
        DateCreated = DateTime.Parse("2025-01-01T00:00:00"),
        DateModified = DateTime.Parse("2025-01-01T00:00:00")
      },
       new Telemetry
      {
        VehicleId = vehicle.Vin,
        Name = TelemetryType.odometer,
        Value = 1010.0f,
        Timestamp = 1735711200, // 2025-01-01 06:00:00
        DateCreated = DateTime.Parse("2025-01-01T06:00:00"),
        DateModified = DateTime.Parse("2025-01-01T06:00:00")
      },
       new Telemetry
      {
        VehicleId = vehicle.Vin,
        Name = TelemetryType.odometer,
        Value = 1020.0f,
        Timestamp = 1735732800, // 2025-01-01 12:00:00
        DateCreated = DateTime.Parse("2025-01-01T12:00:00"),
        DateModified = DateTime.Parse("2025-01-01T12:00:00")
      },
       new Telemetry
      {
        VehicleId = vehicle.Vin,
        Name = TelemetryType.odometer,
        Value = 1030.0f,
        Timestamp = 1735754400, // 2025-01-01 18:00:00
        DateCreated = DateTime.Parse("2025-01-01T18:00:00"),
        DateModified = DateTime.Parse("2025-01-01T18:00:00")
      },
       new Telemetry
      {
        VehicleId = vehicle.Vin,
        Name = TelemetryType.odometer,
        Value = 1040.0f,
        Timestamp = 1735776000, // 2025-01-02 00:00:00
        DateCreated = DateTime.Parse("2025-01-02T00:00:00"),
        DateModified = DateTime.Parse("2025-01-02T00:00:00")
      },
       new Telemetry
      {
        VehicleId = vehicle.Vin,
        Name = TelemetryType.odometer,
        Value = 1050.0f,
        Timestamp = 1735819200, // 2025-01-02 12:00:00
        DateCreated = DateTime.Parse("2025-01-02T12:00:00"),
        DateModified = DateTime.Parse("2025-01-02T12:00:00")
      },
       new Telemetry
      {
        VehicleId = vehicle.Vin,
        Name = TelemetryType.odometer,
        Value = 1060.0f,
        Timestamp = 1735862400, // 2025-01-03 00:00:00
        DateCreated = DateTime.Parse("2025-01-03T00:00:00"),
        DateModified = DateTime.Parse("2025-01-03T00:00:00")
      },
       new Telemetry
      {
        VehicleId = vehicle.Vin,
        Name = TelemetryType.odometer,
        Value = 1070.0f,
        Timestamp = 1735905600, // 2025-01-03 12:00:00
        DateCreated = DateTime.Parse("2025-01-03T12:00:00"),
        DateModified = DateTime.Parse("2025-01-03T12:00:00")
      },
       new Telemetry
      {
        VehicleId = vehicle.Vin,
        Name = TelemetryType.odometer,
        Value = 1080.0f,
        Timestamp = 1735948800, // 2025-01-04 00:00:00
        DateCreated = DateTime.Parse("2025-01-04T00:00:00"),
        DateModified = DateTime.Parse("2025-01-04T00:00:00")
      },
       new Telemetry
      {
        VehicleId = vehicle.Vin,
        Name = TelemetryType.odometer,
        Value = 1090.0f,
        Timestamp = 1735992000, // 2025-01-04 12:00:00
        DateCreated = DateTime.Parse("2025-01-04T12:00:00"),
        DateModified = DateTime.Parse("2025-01-04T12:00:00")
      },
       new Telemetry
      {
        VehicleId = vehicle.Vin,
        Name = TelemetryType.odometer,
        Value = 1100.0f,
        Timestamp = 1736035200, // 2025-01-05 00:00:00
        DateCreated = DateTime.Parse("2025-01-05T00:00:00"),
        DateModified = DateTime.Parse("2025-01-05T00:00:00")
      },
       new Telemetry
      {
        VehicleId = vehicle.Vin,
        Name = TelemetryType.odometer,
        Value = 1110.0f,
        Timestamp = 1736078400, // 2025-01-05 12:00:00
        DateCreated = DateTime.Parse("2025-01-05T12:00:00"),
        DateModified = DateTime.Parse("2025-01-05T12:00:00")
      }
    };

      var batteryTelemetry = new List<Telemetry>
      {
        new Telemetry()
        {
          VehicleId = vehicle.Vin,
          Name = TelemetryType.battery_soc,
          Value = 87.6f,
          Timestamp = 1735689600,  // 2025-01-01 00:00:00 UTC
          DateCreated = new DateTime(2025, 1, 1),
          DateModified = new DateTime(2025, 1, 1)
        },
        new Telemetry()
        {
          VehicleId = vehicle.Vin,
          Name = TelemetryType.battery_soc,
          Value = 83.2f,
          Timestamp = 1735711200,  // 2025-01-01 06:00:00 UTC
          DateCreated = new DateTime(2025, 1, 1),
          DateModified = new DateTime(2025, 1, 1)
        },
        new Telemetry()
        {
          VehicleId = vehicle.Vin,
          Name = TelemetryType.battery_soc,
          Value = 79.1f,
          Timestamp = 1735732800,  // 2025-01-01 12:00:00 UTC
          DateCreated = new DateTime(2025, 1, 1),
          DateModified = new DateTime(2025, 1, 1)
        },
        new Telemetry()
        {
          VehicleId = vehicle.Vin,
          Name = TelemetryType.battery_soc,
          Value = 70.3f,
          Timestamp = 1735754400,  // 2025-01-01 18:00:00 UTC
          DateCreated = new DateTime(2025, 1, 1),
          DateModified = new DateTime(2025, 1, 1)
        },
        new Telemetry()
        {
          VehicleId = vehicle.Vin,
          Name = TelemetryType.battery_soc,
          Value = 58.0f,
          Timestamp = 1735776000,  // 2025-01-02 00:00:00 UTC
          DateCreated = new DateTime(2025, 1, 2),
          DateModified = new DateTime(2025, 1, 2)
        },
        new Telemetry()
        {
          VehicleId = vehicle.Vin,
          Name = TelemetryType.battery_soc,
          Value = 45.7f,
          Timestamp = 1735797600,  // 2025-01-02 06:00:00 UTC
          DateCreated = new DateTime(2025, 1, 2),
          DateModified = new DateTime(2025, 1, 2)
        },
        new Telemetry()
        {
          VehicleId = vehicle.Vin,
          Name = TelemetryType.battery_soc,
          Value = 31.5f,
          Timestamp = 1735819200,  // 2025-01-02 12:00:00 UTC
          DateCreated = new DateTime(2025, 1, 2),
          DateModified = new DateTime(2025, 1, 2)
        },
        new Telemetry()
        {
          VehicleId = vehicle.Vin,
          Name = TelemetryType.battery_soc,
          Value = 18.2f,
          Timestamp = 1735840800,  // 2025-01-02 18:00:00 UTC
          DateCreated = new DateTime(2025, 1, 2),
          DateModified = new DateTime(2025, 1, 2)
        },
        new Telemetry()
        {
          VehicleId = vehicle.Vin,
          Name = TelemetryType.battery_soc,
          Value = 92.8f,
          Timestamp = 1735862400,  // 2025-01-03 00:00:00 UTC (charged)
          DateCreated = new DateTime(2025, 1, 3),
          DateModified = new DateTime(2025, 1, 3)
        },
        new Telemetry()
        {
          VehicleId = vehicle.Vin,
          Name = TelemetryType.battery_soc,
          Value = 86.5f,
          Timestamp = 1735905600,  // 2025-01-03 12:00:00 UTC
          DateCreated = new DateTime(2025, 1, 3),
          DateModified = new DateTime(2025, 1, 3)
        },
        new Telemetry()
        {
          VehicleId = vehicle.Vin,
          Name = TelemetryType.battery_soc,
          Value = 74.2f,
          Timestamp = 1735948800,  // 2025-01-04 00:00:00 UTC
          DateCreated = new DateTime(2025, 1, 4),
          DateModified = new DateTime(2025, 1, 4)
        },
        new Telemetry()
        {
          VehicleId = vehicle.Vin,
          Name = TelemetryType.battery_soc,
          Value = 61.7f,
          Timestamp = 1735992000,  // 2025-01-04 12:00:00 UTC
          DateCreated = new DateTime(2025, 1, 4),
          DateModified = new DateTime(2025, 1, 4)
        }
      };


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
          .Where(r => r.VehicleId == testContext.VehicleVin || r.CustomerId == testContext.FirstCustomerId)
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

      // Remove customer
      var customer = await dbContext.Customers.FindAsync(testContext.FirstCustomerId);
      if (customer != null)
        dbContext.Customers.Remove(customer);

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
