namespace Core.Result
{
  /// <summary>
  /// 
  /// </summary>
  /// <typeparam name="TValue">The type of the value in case of success.</typeparam>
  /// <typeparam name="TError">The type of the error in case of failure.</typeparam>
  public class Result<TValue, TError>
  where TValue : notnull
  where TError : notnull
  {
    private readonly TValue? _value;
    private readonly TError? _error;
    private readonly bool _isSuccess;

    protected Result(TValue value)
    {
      _value = value;
      _error = default;
      _isSuccess = true;
    }

    protected Result(TError error)
    {
      _value = default;
      _error = error;
      _isSuccess = false;
    }

    public static Result<TValue, TError> Success(TValue value) => new Result<TValue, TError>(value);
    public static Result<TValue, TError> Failure(TError error) => new Result<TValue, TError>(error);

    public bool IsSuccess => _isSuccess;
    public bool IsFailure => !_isSuccess;

    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access Value on a failure result.");

    public TError Error => IsFailure
        ? _error!
        : throw new InvalidOperationException("Cannot access Error on a success result.");

    /// <summary>
    /// Matches the result to a success or failure handler function.
    /// </summary>
    public TResult Match<TResult>(
        Func<TValue, TResult> onSuccess,
        Func<TError, TResult> onFailure)
    {
      return IsSuccess
          ? onSuccess(_value!)
          : onFailure(_error!);
    }

    /// <summary>
    /// Executes an action based on whether the result is a success or failure.
    /// </summary>
    public void Match(
        Action<TValue> onSuccess,
        Action<TError> onFailure)
    {
      if (IsSuccess)
        onSuccess(_value!);
      else
        onFailure(_error!);
    }
  }

  /// <summary>
  /// Simplified Result type when the error type is or Error type.
  /// </summary>
  public class Result<TValue> : Result<TValue, Error>
    where TValue : notnull
  {
    private Result(TValue value) : base(value) { }
    private Result(Error error) : base(error) { }

    public new static Result<TValue> Success(TValue value) => new Result<TValue>(value);
    public new static Result<TValue> Failure(Error error) => new Result<TValue>(error);
  }
}

