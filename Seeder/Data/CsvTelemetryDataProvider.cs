using Core.Domain.Entities;
using Core.Interfaces.Data;
using Core.Interfaces.Validation;
using Core.Result;
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
      ITelemetryValidator validator)
    {
      _filePath = options.Value.FileName;
      _logger = logger;
      _validator = validator;
    }
    public Task<Result<IEnumerable<Telemetry>>> GetTelemetryDataAsync(CancellationToken cancellationToken = default)
    {
      throw new NotImplementedException();
    }
  }
}
