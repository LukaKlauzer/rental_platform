namespace Core.Result
{
  public enum ErrorType
  {
    NotFound = 0,
    DatabaseWriteError = 1,
    DatabaseReadError = 2,
    NullReference = 3,
    ValidationError = 4,
    MappingError = 5,
    ProcessingCsv = 6,

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

    public static Error DatabaseWriteError(string message) =>
      new Error("DatabaseWriteError", message, ErrorType.DatabaseWriteError);

    public static Error DatabaseReadError(string message) =>
      new Error("DatabaseReadError", message, ErrorType.DatabaseReadError);

    public static Error NullReferenceError(string message) =>
      new Error("NullReferance", message, ErrorType.NullReference);
    public static Error ValidationError(string message) =>
      new Error("ValidationError", message, ErrorType.ValidationError);
    public static Error MappingError(string message) =>
      new Error("MappingError", message, ErrorType.MappingError);
    public static Error ProcessingCsv(string message) =>
      new Error("ProcessingCsv", message, ErrorType.ProcessingCsv);

  }
}
