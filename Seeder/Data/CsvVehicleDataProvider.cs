using Core.Domain.Entities;
using Core.Interfaces.Data;
using Core.Interfaces.Persistence.SpecificRepository;
using Core.Interfaces.Validation;
using Core.Result;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Seeder.Options;

namespace Seeder.Data
{
  internal class CsvVehicleDataProvider : IVehicleDataProvider
  {
    private readonly ILogger<CsvTelemetryDataProvider> _logger;
    private readonly IVehicleValidator _validator;
    private readonly IVehicleRepository _vehicleRepository;
    private string _filePath;

    public CsvVehicleDataProvider(
      IOptions<CsvDataOptions> options,
      ILogger<CsvTelemetryDataProvider> logger,
      IVehicleValidator validator,
      IVehicleRepository vehicleRepository)
    {
      _filePath = options.Value.VehicleDataFilePath;
      _logger = logger;
      _validator = validator;
      _vehicleRepository = vehicleRepository;
    }

    public async Task<Result<IEnumerable<Vehicle>>> GetVehicleDataAsync(CancellationToken cancellationToken)
    {
      var fileValidation = ValidateFile();
      if (fileValidation.IsFailure)
        return fileValidation;

      try
      {
        using var reader = new StreamReader(_filePath);

        var headerResult = await ReadAndValidateHeaderAsync(reader, cancellationToken);
        if (headerResult.IsFailure)
          return headerResult;

        var allCsvVehicleRecords = await ProcessDataRowsAsync(reader, cancellationToken);


        // Retrieve all vehicles to validate if their telemetry records are valid
        var allExistingVehiclesResult = await _vehicleRepository.GetAll();
        if (allExistingVehiclesResult.IsFailure)
          return Result<IEnumerable<Vehicle>>.Failure(Error.DatabaseReadError("Failed to retrieve vehicles for telemetry validation: " +
                                        allExistingVehiclesResult.Error.Message));

        var distinctExistingVins = allExistingVehiclesResult.Value.Select(x => x.Vin).Distinct().ToList();
        if (distinctExistingVins is null)
          return Result<IEnumerable<Vehicle>>.Failure(Error.NullReferenceError("No vehicles found in the database. Cannot validate telemetry data."));

        var validVehicles = _validator.FilterValidVehicles(allCsvVehicleRecords);

        var nonExistingVehacles = validVehicles.Where(v => !distinctExistingVins.Contains(v.Vin)).ToList();

        return Result<IEnumerable<Vehicle>>.Success(nonExistingVehacles);
      }
      catch (Exception ex)
      {
        return Result<IEnumerable<Vehicle>>.Failure(Error.ProcessingCsv($"Error occurred while processing vehicle csv file: {ex.Message}"));
      }
    }

    private Result<IEnumerable<Vehicle>> ValidateFile()
    {
      if (string.IsNullOrEmpty(_filePath))
        return Result<IEnumerable<Vehicle>>.Failure(
            Error.ValidationError("Vehicle CSV file path is not configured in application settings."));

      if (!File.Exists(_filePath))
        return Result<IEnumerable<Vehicle>>.Failure(
            Error.NotFound($"Vehicle CSV file not found: {_filePath}"));

      return Result<IEnumerable<Vehicle>>.Success(Enumerable.Empty<Vehicle>());
    }

    private async Task<Result<IEnumerable<Vehicle>>> ReadAndValidateHeaderAsync(StreamReader reader, CancellationToken cancellationToken)
    {
      string? headerLine = await reader.ReadLineAsync(cancellationToken);
      if (headerLine == null)
        return Result<IEnumerable<Vehicle>>.Success(Enumerable.Empty<Vehicle>());

      // Define the expected headers and their order
      string[] expectedHeaders = { "vin", "make", "model", "year", "priceperkmineuro", "priceperdayineuro" };
      string[] headers = headerLine.Split(',').Select(h => h.Trim().ToLower()).ToArray();

      // Validate header count
      var headerCountValidation = ValidateHeaderCount(headers, expectedHeaders);
      if (headerCountValidation.IsFailure)
        return headerCountValidation;

      // Validate header order
      var headerOrderValidation = ValidateHeaderOrder(headers, expectedHeaders);
      if (headerOrderValidation.IsFailure)
        return headerOrderValidation;

      return Result<IEnumerable<Vehicle>>.Success(Enumerable.Empty<Vehicle>());
    }

    private Result<IEnumerable<Vehicle>> ValidateHeaderCount(string[] headers, string[] expectedHeaders)
    {
      if (headers.Length != expectedHeaders.Length)
      {
        _logger.LogError("CSV file has {ActualCount} columns, but expected {ExpectedCount}",
            headers.Length, expectedHeaders.Length);
        return Result<IEnumerable<Vehicle>>.Failure(
            Error.ValidationError("CSV file has an incorrect number of columns."));
      }

      return Result<IEnumerable<Vehicle>>.Success(Enumerable.Empty<Vehicle>());
    }

    private Result<IEnumerable<Vehicle>> ValidateHeaderOrder(string[] headers, string[] expectedHeaders)
    {
      for (int i = 0; i < expectedHeaders.Length; i++)
        if (headers[i] != expectedHeaders[i])
        {
          _logger.LogError("CSV header mismatch at position {Position}. Expected: {Expected}, Found: {Actual}",
              i, expectedHeaders[i], headers[i]);
          return Result<IEnumerable<Vehicle>>.Failure(
              Error.ValidationError($"CSV header format is invalid. Expected '{expectedHeaders[i]}' at position {i}, but found '{headers[i]}'."));

        }
      return Result<IEnumerable<Vehicle>>.Success(Enumerable.Empty<Vehicle>());
    }

    private async Task<List<Vehicle>> ProcessDataRowsAsync(StreamReader reader, CancellationToken cancellationToken)
    {
      List<Vehicle> vehicleRecords = new List<Vehicle>();
      string[] expectedHeaders = { "vin", "make", "model", "year", "priceperkminuro", "priceperdayineuro" };
      int rowNumber = 1; // Start at 1
      string? line;

      while ((line = await reader.ReadLineAsync(cancellationToken)) != null)
      {
        rowNumber++;

        if (string.IsNullOrWhiteSpace(line))
          continue;

        var vehicle = ParseRow(line, rowNumber, expectedHeaders.Length);
        if (vehicle != null)
          vehicleRecords.Add(vehicle);

      }
      return vehicleRecords;
    }

    private Vehicle? ParseRow(string line, int rowNumber, int expectedFieldCount)
    {
      string[] fields = line.Split(',').Select(f => f.Trim()).ToArray();

      // Ensure the row has the correct number of fields
      if (fields.Length != expectedFieldCount)
      {
        _logger.LogWarning("Row {RowNumber} has {FieldCount} fields, expected {ExpectedCount}. Skipping.",
            rowNumber, fields.Length, expectedFieldCount);
        return null;
      }

      try
      {
        return new Vehicle
        {
          Vin = fields[0],
          Make = fields[1],
          Model = fields[2],
          Year = int.Parse(fields[3]),
          PricePerKmInEuro = float.Parse(fields[4]),
          PricePerDayInEuro = float.Parse(fields[5])
        };
      }
      catch (Exception ex)
      {
        _logger.LogWarning(ex, "Error parsing row {RowNumber}. Skipping.", rowNumber);
        return null;
      }
    }

  }
}
