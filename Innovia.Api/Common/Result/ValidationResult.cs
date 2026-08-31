using System.Text.Json;
using Innovia.Api.Common.Errors;

namespace Innovia.Api.Common.Result;

public sealed class ValidationResult
{
    public bool IsValid { get; }
    public IReadOnlyList<ValidationError> Errors { get; }

    private ValidationResult(bool isValid, IReadOnlyList<ValidationError> errors)
    {
        IsValid = isValid;
        Errors = errors;
    }

    public static ValidationResult Success() => new(true, []);
    public static ValidationResult Fail(IReadOnlyList<ValidationError> errors) => new(false, errors);

}

public static class ValidationResultExtensions
{
    public static IResult ToProblemResult(this ValidationResult validationResult)
    {
        var errors = validationResult.Errors    
            .GroupBy(e => JsonNamingPolicy.CamelCase.ConvertName(e.Field))
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.Message).ToArray()
            );

            return Results.ValidationProblem(errors);
    }
}