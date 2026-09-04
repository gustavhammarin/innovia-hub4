using Innovia.Api.Common.Errors;
using Innovia.Api.Common.Result;

namespace Innovia.Api.Features.ResourceTypes.UpdateResourceType;

public sealed class Validator
{
    public ValidationResult Validate(Command command)
    {
        var errors = new List<ValidationError>();

        if (command.Id == Guid.Empty)
            errors.Add(new ValidationError(nameof(command.Id), "Id is required"));

        if (string.IsNullOrWhiteSpace(command.Name))
            errors.Add(new ValidationError(nameof(command.Name), "Name is required"));

        if (command.Name is { Length: > 100 })
            errors.Add(new ValidationError(nameof(command.Name), "Name cannot exceed 100 characters"));

        if (command.MaxDurationMinutes <= 0)
            errors.Add(new ValidationError(nameof(command.MaxDurationMinutes), "MaxDurationMinutes must be greater than 0"));

        if (command.MaxAdvanceDays <= 0)
            errors.Add(new ValidationError(nameof(command.MaxAdvanceDays), "MaxAdvanceDays must be greater than 0"));

        return errors.Count == 0
            ? ValidationResult.Success()
            : ValidationResult.Fail(errors);
    }
}
