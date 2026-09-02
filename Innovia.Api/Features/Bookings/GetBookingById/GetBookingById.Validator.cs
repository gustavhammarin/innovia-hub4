using Innovia.Api.Common.Errors;
using Innovia.Api.Common.Result;

namespace Innovia.Api.Features.Bookings.GetBookingById;

public sealed class Validator
{
    public ValidationResult Validate (Command command)
    {
        var errors = new List<ValidationError>();

        if(command.BookingId == Guid.Empty)
            errors.Add(new ValidationError(nameof(command.BookingId), "Booking Id is required"));

        return errors.Count == 0
            ? ValidationResult.Success()
            : ValidationResult.Fail(errors);
    }
}