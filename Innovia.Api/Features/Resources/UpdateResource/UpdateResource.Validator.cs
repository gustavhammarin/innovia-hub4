using Innovia.Api.Common.Errors;
using Innovia.Api.Common.Result;

namespace Innovia.Api.Features.Resources.UpdateResource;

public sealed class Validator
{
    public ValidationResult Validate (Command command)
    {
        var errors = new List <ValidationError>();

        if (command.Id == Guid.Empty)
            errors.Add(new ValidationError(nameof(command.Id), "Id is required"));

        if(string.IsNullOrWhiteSpace(command.Name))
            errors.Add(new ValidationError(nameof(command.Name), "Name is required"));

        if(command.ResourceTypeId == Guid.Empty)
            errors.Add(new ValidationError(nameof(command.ResourceTypeId), "Resource Type Id is required"));

        return errors.Count == 0 
        ? ValidationResult.Success()
        : ValidationResult.Fail(errors);
    } 
}