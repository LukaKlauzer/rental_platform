using Core.Domain.Entities;
using Core.Interfaces.Data;
using Core.Interfaces.Validation;
using Core.Result;
using Microsoft.Extensions.Logging;

namespace Seeder.Data
{
  internal class CsvVehicleDataProvider : IVehicleDataProvider
  {
    private readonly ILogger<CsvTelemetryDataProvider> _logger;
    private readonly ITelemetryValidator _validator;
    private string _filePath;

    public CsvVehicleDataProvider(
      ILogger<CsvTelemetryDataProvider> logger,
      ITelemetryValidator validator)
    {
      _logger = logger;
      _validator = validator;
    }
    public Task<Result<IEnumerable<Telemetry>>> GetVehicleDataAsync(CancellationToken cancellationToken = default)
    {
      throw new NotImplementedException();
    }
  }
}
