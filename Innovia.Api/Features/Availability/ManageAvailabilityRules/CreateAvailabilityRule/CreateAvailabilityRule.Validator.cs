using Innovia.Api.Common.Errors;
using Innovia.Api.Common.Result;

namespace Innovia.Api.Features.Availability.ManageAvailabilityRules.CreateAvailabilityRule;

public sealed class Validator
{
    public ValidationResult Validate(Command command)
    {
        var errors = new List<ValidationError>();

        if (command.ResourceTypeId == Guid.Empty)
            errors.Add(new ValidationError(nameof(command.ResourceTypeId), "ResourceTypeId is required"));

        if (!Enum.IsDefined(command.DayOfWeek))
            errors.Add(new ValidationError(nameof(command.DayOfWeek), "DayOfWeek is invalid"));

        if (command.OpensAt >= command.ClosesAt)
            errors.Add(new ValidationError(nameof(command.OpensAt), "OpensAt must be before ClosesAt"));

        if (command.SlotDurationMinutes <= 0)
            errors.Add(new ValidationError(nameof(command.SlotDurationMinutes), "SlotDurationMinutes must be greater than 0"));

        if (command.OpensAt < command.ClosesAt && command.SlotDurationMinutes > 0)
        {
            var openMinutes = (command.ClosesAt.ToTimeSpan() - command.OpensAt.ToTimeSpan()).TotalMinutes;
            if (openMinutes < command.SlotDurationMinutes || openMinutes % command.SlotDurationMinutes != 0)
                errors.Add(new ValidationError(nameof(command.SlotDurationMinutes), "SlotDurationMinutes must fit inside the opening window"));
        }

        return errors.Count == 0
            ? ValidationResult.Success()
            : ValidationResult.Fail(errors);
    }
}
