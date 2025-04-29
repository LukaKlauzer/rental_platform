using Core.Domain.Entities;
using Core.Enums;
using Core.Interfaces.Data;
using Core.Interfaces.Persistence.SpecificRepository;
using Core.Interfaces.Validation;
using Core.Result;
using Infrastructure.Persistance.ConcreteRepositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Seeder.Options;

namespace Seeder.Data
{
  internal class CsvTelemetryDataProvider : ITelemetryDataProvider
  {
    private readonly ILogger<CsvTelemetryDataProvider> _logger;
    private readonly ITelemetryValidator _validator;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly ITelemetryRepository _telemetryRepository;
    private string _filePath;

    public CsvTelemetryDataProvider(
      IOptions<CsvDataOptions> options,
      ILogger<CsvTelemetryDataProvider> logger,
      ITelemetryValidator validator,
      IVehicleRepository vehicleRepository,
      ITelemetryRepository telemetryRepository
      )
    {
      _filePath = options.Value.TelemetryDataFilePath;
      _logger = logger;
      _validator = validator;
      _vehicleRepository = vehicleRepository;
      _telemetryRepository = telemetryRepository;
    }
    public async Task<Result<IEnumerable<Telemetry>>> GetTelemetryDataAsync(CancellationToken cancellationToken = default)
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

        var allCsvTelemetryRecords = await ProcessDataRowsAsync(reader, cancellationToken);

        // Retrieve all vehicles to validate if their telemetry records are valid
        var allExistingVehiclesResult = await _vehicleRepository.GetAll();

        if (allExistingVehiclesResult.IsFailure)
          return Result<IEnumerable<Telemetry>>.Failure(Error.DatabaseReadError("Failed to retrieve vehicles for telemetry validation: " +
                                        allExistingVehiclesResult.Error.Message));

        var distinctVins = allExistingVehiclesResult.Value.Select(x => x.Vin).Distinct().ToList();
        if (distinctVins is null)
          return Result<IEnumerable<Telemetry>>.Failure(Error.NullReferenceError("No vehicles found in the database. Cannot validate telemetry data."));

        var filteredTelemetry = _validator.FilterValidTelemetry(allCsvTelemetryRecords);

        var missingTelemetry = await _telemetryRepository.GetMissingRecords(filteredTelemetry);

        if (missingTelemetry.IsFailure)
          return Result<IEnumerable<Telemetry>>.Failure(Error.DatabaseReadError($"Filed while retreaving telemetry onjunction: {missingTelemetry.Error}"));

        return Result<IEnumerable<Telemetry>>.Success(missingTelemetry.Value);

      }
      catch (Exception ex)
      {
        return Result<IEnumerable<Telemetry>>.Failure(Error.ProcessingCsv($"Error occurred while processing telemetry csv file: {ex.Message}"));
      }
    }

    private Result<IEnumerable<Telemetry>> ValidateFile()
    {
      if (string.IsNullOrEmpty(_filePath))
        return Result<IEnumerable<Telemetry>>.Failure(
            Error.ValidationError("Telemetry CSV file path is not configured in application settings."));

      if (!File.Exists(_filePath))
        return Result<IEnumerable<Telemetry>>.Failure(
            Error.NotFound($"Telemetry CSV file not found: {_filePath}"));

      return Result<IEnumerable<Telemetry>>.Success(Enumerable.Empty<Telemetry>());
    }

    private async Task<Result<IEnumerable<Telemetry>>> ReadAndValidateHeaderAsync(StreamReader reader, CancellationToken cancellationToken)
    {
      string? headerLine = await reader.ReadLineAsync(cancellationToken);
      if (headerLine == null)
        return Result<IEnumerable<Telemetry>>.Success(Enumerable.Empty<Telemetry>());

      // Define the expected headers and their order
      string[] expectedHeaders = { "vin", "name", "value", "timestamp" };
      string[] headers = headerLine.Split(',').Select(h => h.Trim().ToLower()).ToArray();

      // Validate header count
      var headerCountValidation = ValidateHeaderCount(headers, expectedHeaders);
      if (headerCountValidation.IsFailure)
        return headerCountValidation;

      // Validate header values and positions
      var headerOrderValidation = ValidateHeaderOrder(headers, expectedHeaders);
      if (headerOrderValidation.IsFailure)
        return headerOrderValidation;

      return Result<IEnumerable<Telemetry>>.Success(Enumerable.Empty<Telemetry>());
    }
    private Result<IEnumerable<Telemetry>> ValidateHeaderCount(string[] headers, string[] expectedHeaders)
    {
      if (headers.Length != expectedHeaders.Length)
      {
        _logger.LogError("CSV file has {ActualCount} columns, but expected {ExpectedCount}",
            headers.Length, expectedHeaders.Length);
        return Result<IEnumerable<Telemetry>>.Failure(
            Error.ValidationError("CSV file has an incorrect number of columns."));
      }

      return Result<IEnumerable<Telemetry>>.Success(Enumerable.Empty<Telemetry>());
    }

    private Result<IEnumerable<Telemetry>> ValidateHeaderOrder(string[] headers, string[] expectedHeaders)
    {
      for (int i = 0; i < expectedHeaders.Length; i++)
        if (headers[i] != expectedHeaders[i])
        {
          _logger.LogError("CSV header mismatch at position {Position}. Expected: {Expected}, Found: {Actual}",
              i, expectedHeaders[i], headers[i]);
          return Result<IEnumerable<Telemetry>>.Failure(
              Error.ValidationError($"CSV header format is invalid. Expected '{expectedHeaders[i]}' at position {i}, but found '{headers[i]}'."));
        }
      return Result<IEnumerable<Telemetry>>.Success(Enumerable.Empty<Telemetry>());
    }

    private async Task<List<Telemetry>> ProcessDataRowsAsync(StreamReader reader, CancellationToken cancellationToken)
    {
      List<Telemetry> telemetryRecords = new List<Telemetry>();
      string[] expectedHeaders = { "vin", "name", "value", "timestamp" };
      int rowNumber = 1; // Start at 1 
      string? line;

      while ((line = await reader.ReadLineAsync(cancellationToken)) != null)
      {
        rowNumber++;

        if (string.IsNullOrWhiteSpace(line))
          continue;

        var telemetry = ParseRow(line, rowNumber, expectedHeaders.Length);
        if (telemetry != null)
          telemetryRecords.Add(telemetry);

      }
      return telemetryRecords;
    }
    private Telemetry? ParseRow(string line, int rowNumber, int expectedFieldCount)
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
        return new Telemetry
        {
          VehicleId = fields[0],
          Name = ParseTelemetryType(fields[1]),
          Value = float.Parse(fields[2]),
          Timestamp = int.Parse(fields[3])
        };
      }
      catch (Exception ex)
      {
        _logger.LogWarning(ex, "Error parsing row {RowNumber}. Skipping.", rowNumber);
        return null;
      }
    }
    private TelemetryType ParseTelemetryType(string typeString)
    {
      return typeString.ToLower() switch
      {
        "odometer" => TelemetryType.odometer,
        "battery_soc" => TelemetryType.battery_soc,
        _ => TelemetryType.Unknown
      };
    }
  }
}
