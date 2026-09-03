using Innovia.Api.Common.Errors;
using Innovia.Api.Common.Result;

namespace Innovia.Api.Features.Availability.ManageAvailabilityRules.ListAvailabilityRules;

public sealed class Validator
{
    public ValidationResult Validate(Command command)
    {
        var errors = new List<ValidationError>();

        if (command.ResourceTypeId == Guid.Empty)
            errors.Add(new ValidationError(nameof(command.ResourceTypeId), "ResourceTypeId is required"));

        return errors.Count == 0
            ? ValidationResult.Success()
            : ValidationResult.Fail(errors);
    }
}
