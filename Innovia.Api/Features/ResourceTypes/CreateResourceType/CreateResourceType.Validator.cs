using Innovia.Api.Common.Errors;
using Innovia.Api.Common.Result;

namespace Innovia.Api.Features.ResourceTypes.CreateResourceType;

public sealed class Validator
{
    public ValidationResult Validate(Command command)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(command.Name))
            errors.Add(new ValidationError(nameof(command.Name), "Name is required"));

        if (command.Name is { Length: > 100 })
            errors.Add(new ValidationError(nameof(command.Name), "Name cannot exceed 100 characters"));

        return errors.Count == 0
            ? ValidationResult.Success()
            : ValidationResult.Fail(errors);
    }
}
