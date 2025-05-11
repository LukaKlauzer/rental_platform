using System.Collections.Generic;
using System.Globalization;
using Application.Interfaces.Data;
using Core.Domain.Entities;
using Core.Interfaces.Validation;
using Core.Result;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Seeder.Options;

namespace Seeder.Data
{
  internal class CsvVehicleDataProvider : IVehicleDataProvider
  {
    private readonly ILogger<CsvVehicleDataProvider> _logger;
    private readonly IVehicleValidator _validator;
    private string _filePath;

    public CsvVehicleDataProvider(
      IOptions<CsvDataOptions> options,
      ILogger<CsvVehicleDataProvider> logger,
      IVehicleValidator validator)
    {
      _filePath = options.Value.VehicleDataFilePath;
      _logger = logger;
      _validator = validator;
    }

    public async Task<Result<IEnumerable<Vehicle>>> GetVehicleDataAsync(CancellationToken cancellationToken = default)
    {
      var vehicles = new List<Vehicle>();
      var totalRows = 0;
      var skippedRows = 0;

      try
      {
        using var reader = new StreamReader(_filePath);
        using var csv = new CsvReader(reader, GetCsvConfiguration());

        if (csv.Configuration.HasHeaderRecord)
        {
          await csv.ReadAsync();
          csv.ReadHeader();
        }

        // Manually read each record to handle row-by-row errors
        while (await csv.ReadAsync())
        {
          totalRows++;

          try
          {
            var record = csv.GetRecord<VehicleCsvDto>();
            var vehicleResult = CreateVehicle(record);

            if (vehicleResult.IsSuccess)
            {
              vehicles.Add(vehicleResult.Value);
            }
            else
            {
              skippedRows++;
              _logger.LogWarning("Failed to create vehicle at row {Row}: {Error}",
                  csv.Context.Parser?.Row, vehicleResult.Error.Message);
            }
          }
          catch (Exception ex)
          {
            skippedRows++;
            _logger.LogWarning(ex, "Failed to parse row {Row}: {Message}",
                csv.Context.Parser?.Row, ex.Message);
          }
        }

        _logger.LogInformation("CSV processing complete. Total rows: {Total}, Valid: {Valid}, Skipped: {Skipped}",
            totalRows, vehicles.Count, skippedRows);

        // Filter using your validator => not necesery
        var validVehicles = _validator.FilterValidVehicles(vehicles);

        return Result<IEnumerable<Vehicle>>.Success(validVehicles);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Critical error reading vehicle CSV file");
        return Result<IEnumerable<Vehicle>>.Failure(
            Error.ProcessingCsv($"Failed to process vehicle CSV: {ex.Message}"));
      }
    }
    private Result<Vehicle> CreateVehicle(VehicleCsvDto dto)
    {
      return Vehicle.Create(
         vin: dto.Vin,
         make: dto.Make,
         model: dto.Model,
         year: dto.Year,
         pricePerKmInEuro: dto.PricePerKmInEuro,
         pricePerDayInEuro: dto.PricePerDayInEuro
      );
    }

    private CsvConfiguration GetCsvConfiguration()
    {
      return new CsvConfiguration(CultureInfo.InvariantCulture)
      {
        TrimOptions = TrimOptions.Trim,
        HasHeaderRecord = true,

        MissingFieldFound = (args) =>
        {
          if (args.HeaderNames is null)
            return;

          var missingField = string.Join(", ", args.HeaderNames ?? Array.Empty<string>());
          var message = string.Format("Missing field at row: {Row}, position: {Index}. Expected fields: {Fields}",
              args.Context.Parser?.Row, args.Index, missingField);
          _logger.LogWarning(message);

        },
        BadDataFound = context =>
        {
          // I dont think this onw is been used
          _logger.LogWarning("Unable to parse data found at row {Row}: {RawRecord}",
              context.Context.Parser?.Row, context.RawRecord);
          return;
        },
        ReadingExceptionOccurred = args =>
        {
          _logger.LogError(args.Exception, "CSV reading error at row {Row}",
              args.Exception.Context?.Parser?.Row);

          // Continue processing (return true) or stop (return false)
          return true;
        }
      };
    }
  }
}
