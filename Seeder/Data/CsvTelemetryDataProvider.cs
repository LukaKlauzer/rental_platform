using System.Globalization;
using System.Text.RegularExpressions;
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
  internal class CsvTelemetryDataProvider : ITelemetryDataProvider
  {
    private readonly ILogger<CsvTelemetryDataProvider> _logger;
    private readonly ITelemetryValidator _validator;
    private string _filePath;

    public CsvTelemetryDataProvider(
      IOptions<CsvDataOptions> options,
      ILogger<CsvTelemetryDataProvider> logger,
      ITelemetryValidator validator
      )
    {
      _filePath = options.Value.TelemetryDataFilePath;
      _logger = logger;
      _validator = validator;
    }

    public async Task<Result<IEnumerable<Telemetry>>> GetTelemetryDataAsync(CancellationToken cancellationToken = default)
    {
      var telemetries = new List<Telemetry>();
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
        while (await csv.ReadAsync())
        {
          totalRows++;
          try
          {
            var record = csv.GetRecord<TelemetryCsvDto>();
            var telemetryResult = CreateTelemetry(record);
            if (telemetryResult.IsSuccess)
              telemetries.Add(telemetryResult.Value);
            else
            {
              skippedRows++;
              _logger.LogWarning("Failed to create telemetry at {Row}: {Error}",
                csv.Context.Parser?.Row, telemetryResult.Error.Message);
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
            totalRows, telemetries.Count, skippedRows);

        // Filter using your validator => not necesery
        var validTelemetries = _validator.FilterValidTelemetry(telemetries);

        return Result<IEnumerable<Telemetry>>.Success(validTelemetries);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Critical error reading telemetry CSV file");
        return Result<IEnumerable<Telemetry>>.Failure(
            Error.ProcessingCsv($"Failed to process telemetry CSV: {ex.Message}"));
      }
    }
    private Result<Telemetry> CreateTelemetry(TelemetryCsvDto dto)
    {
      return Telemetry.Create(
          telemetryType: dto.Name,
          value: dto.Value,
          timestamp: dto.Timestamp,
          vehicleId: dto.VehicleId
      );
    }

    private CsvConfiguration GetCsvConfiguration()
    {
      return new CsvConfiguration(CultureInfo.InvariantCulture)
      {
        TrimOptions = TrimOptions.Trim,
        HasHeaderRecord = true,
        HeaderValidated = (args) =>
        {
          if (!args.InvalidHeaders.Any())
            return;

          var invalidHeaders = string.Join(", ", args.InvalidHeaders.ToList());

          _logger.LogWarning("Invalid headers found: {Headers}", invalidHeaders);
        },
        MissingFieldFound = (args) =>
        {
          if (args.HeaderNames is null)
            return;
          var missingField = string.Join(", ", args.HeaderNames);

          _logger.LogWarning("Missing field at row: {Row}, position: {Index}. Expected fields: {Fields}", args.Context.Parser?.Row, args.Index, missingField);
        },
        BadDataFound = context =>
        {
          _logger.LogWarning("Unable to parse data found at row {Row}: {RawRecord}",
              context.Context.Parser?.Row, context.RawRecord);
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
