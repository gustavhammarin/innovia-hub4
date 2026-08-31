using Innovia.Api.Common.Errors;
using Innovia.Api.Common.Result;

namespace Innovia.Api.Features.Bookings.CreateBooking;

public sealed class Validator
{
    public ValidationResult Validate(Command command)
    {
        var errors = new List<ValidationError>();

        if (command.ResourceId == Guid.Empty)
            errors.Add(new ValidationError(nameof(command.ResourceId), "ResourceId is required"));

        if (command.StartsAt >= command.EndsAt)
            errors.Add(new ValidationError(nameof(command.StartsAt), "StartAt must be before EndsAt"));

        if (command.StartsAt < DateTimeOffset.UtcNow)
            errors.Add(new ValidationError(nameof(command.StartsAt), "Cannot book a time in the past"));

        var duration = command.EndsAt - command.StartsAt;
        if (duration > TimeSpan.FromHours(8))
            errors.Add(new ValidationError(nameof(command.EndsAt), "Booking cannot exceed 8 hours"));

        return errors.Count == 0
            ? ValidationResult.Success()
            : ValidationResult.Fail(errors);
    }
}