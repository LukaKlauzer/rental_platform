namespace Core.Result
{
  public enum ErrorType
  {
    NotFound = 0,
    InvalidFileName = 1,
    StorageError = 2,
    NullReference = 3,

  }

  public class Error
  {
    private Error(
        string code,
        string message,
        ErrorType errorType
    )
    {
      Code = code;
      Message = message;
      ErrorType = errorType;
    }

    public string Code { get; }
    public string Message { get; }
    public ErrorType ErrorType { get; }

    public static Error NotFound(string message) =>
      new("NotFound", message, ErrorType.NotFound);

    public static Error InvalidFileName(string message) =>
      new Error("InvalidFileName", message, ErrorType.InvalidFileName);

    public static Error StorageError(string message) =>
      new Error("StorageError", message, ErrorType.StorageError);
    public static Error NullReferenceError(string message) =>
      new Error("NullReferance", message, ErrorType.NullReference);

  }
}
