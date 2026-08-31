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

        if (command.StartAt >= command.EndsAt)
            errors.Add(new ValidationError(nameof(command.StartAt), "StartAt must be before EndsAt"));

        if (command.StartAt < DateTimeOffset.UtcNow)
            errors.Add(new ValidationError(nameof(command.StartAt), "Cannot book a time in the past"));

        var duration = command.EndsAt - command.StartAt;
        if (duration > TimeSpan.FromHours(8))
            errors.Add(new ValidationError(nameof(command.EndsAt), "Booking cannot exceed 8 hours"));

        return errors.Count == 0
            ? ValidationResult.Success()
            : ValidationResult.Fail(errors);
    }
}