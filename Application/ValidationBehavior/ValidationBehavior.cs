using Core.Result;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.ValidationBehavior
{
  public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, Result<TResponse>>
     where TRequest : IRequest<Result<TResponse>>
     where TResponse : notnull
  {
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger;

    public ValidationBehavior(
        IEnumerable<IValidator<TRequest>> validators,
        ILogger<ValidationBehavior<TRequest, TResponse>> logger)
    {
      _validators = validators;
      _logger = logger;
    }

    public async Task<Result<TResponse>> Handle(
        TRequest request,
        RequestHandlerDelegate<Result<TResponse>> next,
        CancellationToken cancellationToken)
    {

      if (!_validators.Any())
        return await next();
      

      var context = new ValidationContext<TRequest>(request);

      var validationResults = await Task.WhenAll(
          _validators.Select(v =>
          {
            return v.ValidateAsync(context, cancellationToken);
          }));

      var failures = validationResults
          .SelectMany(r => r.Errors)
          .Where(f => f != null)
          .ToList();

      if (failures.Count == 0)
        return await next();

      var errorMessage = string.Join("; ", failures.Select(x => x.ErrorMessage));

      return Result<TResponse>.Failure(Error.ValidationError(errorMessage));
    }
  }
}
