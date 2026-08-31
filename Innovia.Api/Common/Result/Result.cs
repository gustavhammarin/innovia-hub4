using Innovia.Api.Common.Errors;

namespace Innovia.Api.Common.Result;

public sealed class Result<T>
{
    public bool IsSuccess {get;}
    public T? Value {get;}
    public Error? Error {get;}

    private Result(bool isSuccess, T? value, Error? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public static Result<T> Ok(T value) => new(true, value, null);
    public static Result<T> Fail(Error error) => new(false, default, error);
}

public static class ResultExtensions
{
    public static IResult ToHttpResponse<T>(this Result<T> result)
    {
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : result.Error!.ToProblemResult();
    }
}