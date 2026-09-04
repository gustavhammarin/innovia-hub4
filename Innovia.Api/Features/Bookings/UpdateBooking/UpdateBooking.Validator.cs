using Innovia.Api.Common.Errors;
using Innovia.Api.Common.Result;

namespace Innovia.Api.Features.Bookings.UpdateBooking;

public sealed class Validator
{
    public ValidationResult Validate(Command command)
    {
        var errors = new List<ValidationError>();

        if (command.BookingId == Guid.Empty)
            errors.Add(new ValidationError(nameof(command.BookingId), "BookingId is required"));

        if (command.ResourceId == Guid.Empty)
            errors.Add(new ValidationError(nameof(command.ResourceId), "Resource id is required."));

        if (command.UserId == Guid.Empty)
            errors.Add(new ValidationError(nameof(command.UserId), "UserId is required"));

        if (command.StartsAt >= command.EndsAt)
            errors.Add(new ValidationError(nameof(command.StartsAt), "Start time must be before end time."));

        return errors.Count == 0
            ? ValidationResult.Success()
            : ValidationResult.Fail(errors);
    }
}
