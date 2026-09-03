using Innovia.Api.Common.Errors;
using Innovia.Api.Common.Result;

namespace Innovia.Api.Features.Availability.GetResourceAvailability;

public sealed class Validator
{
    private const int MaxRangeDays = 31;

    public ValidationResult Validate(Command command)
    {
        var errors = new List<ValidationError>();

        if (command.ResourceId == Guid.Empty)
            errors.Add(new ValidationError(nameof(command.ResourceId), "ResourceId is required"));

        if (command.FromDate > command.ToDate)
            errors.Add(new ValidationError(nameof(command.FromDate), "FromDate must be before or equal to ToDate"));

        if (command.FromDate <= command.ToDate)
        {
            var rangeDays = command.ToDate.DayNumber - command.FromDate.DayNumber + 1;
            if (rangeDays > MaxRangeDays)
                errors.Add(new ValidationError(nameof(command.ToDate), $"Date range cannot exceed {MaxRangeDays} days"));
        }

        return errors.Count == 0
            ? ValidationResult.Success()
            : ValidationResult.Fail(errors);
    }
}
