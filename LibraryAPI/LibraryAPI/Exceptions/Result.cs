namespace LibraryAPI.Exceptions;

public class Result<T>
{
    public bool IsSuccess { get; }
    public T Value { get; }
    public string ErrorMessage { get; }
    public int? StatusCode { get; }
    public Exception Exception { get; }

    private Result(bool isSuccess, T value, string errorMessage, int? statusCode, Exception exception)
    {
        IsSuccess = isSuccess;
        Value = value;
        ErrorMessage = errorMessage;
        StatusCode = statusCode;
        Exception = exception;
    }

    public static Result<T> Success(T value)
    {
        return new Result<T>(true, value, null, null, null);
    }

    public static Result<T> Failure(string errorMessage, int statusCode, Exception exception = null)
    {
        return new Result<T>(false, default, errorMessage, statusCode, exception);
    }
}

public class Result
{
    public bool IsSuccess { get; }
    public string ErrorMessage { get; }
    public Exception Exception { get; }
    public int? StatusCode { get; }

    private Result(bool isSuccess, string errorMessage, int? statusCode, Exception exception)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        Exception = exception;
        StatusCode = statusCode;
    }

    public static Result Success()
    {
        return new Result(true, null, null, null);
    }

    public static Result Failure(string errorMessage,int statusCode, Exception exception = null)
    {
        return new Result(false, errorMessage, statusCode, exception);
    }
}