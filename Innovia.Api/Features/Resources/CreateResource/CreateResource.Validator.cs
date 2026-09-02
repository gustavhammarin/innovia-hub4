using Innovia.Api.Common.Errors;
using Innovia.Api.Common.Result;

namespace Innovia.Api.Features.Resources.CreateResource;

public sealed class Validator
{
    public ValidationResult Validate(Command command)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(command.Name))
            errors.Add(new ValidationError(nameof(command.Name), "Name is required"));

        if (command.Name.Length > 100)
            errors.Add(new ValidationError(nameof(command.Name), "Name cannot exceed 100 characters"));

        if (string.IsNullOrWhiteSpace(command.Description))
            errors.Add(new ValidationError(nameof(command.Description), "Description is required"));

        if (command.Description.Length > 1_000)
            errors.Add(new ValidationError(nameof(command.Description), "Description cannot exceed 1000 characters"));

        if (command.ResourceTypeId == Guid.Empty)
            errors.Add(new ValidationError(nameof(command.ResourceTypeId), "ResourceTypeId is required"));

        return errors.Count == 0
            ? ValidationResult.Success()
            : ValidationResult.Fail(errors);
    }
}
