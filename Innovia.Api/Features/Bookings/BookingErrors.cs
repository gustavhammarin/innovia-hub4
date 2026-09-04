using Innovia.Api.Common.Errors;
using Innovia.Api.Common.Database.Entities;

namespace Innovia.Api.Features.Bookings;

public static class BookingErrors
{
    public static readonly Error Overlaps = Error.Conflict(
        "This resource is already booked for the selected time"
    );

    public static Error InvalidReference() => Error.NotFound(
        "Invalid reference"
    );

    public static readonly Error NotFound = Error.NotFound(
        "Booking Not Found"
    );

    public static readonly Error NotAuthorized = Error.Forbidden(
        "You are not authorized to view this user's bookings"
    );

    public static readonly Error NotAuthorizedToUpdate = Error.Forbidden(
        "You are not authorized to update this booking"
    );

    public static readonly Error CannotBookInPast = Error.Validation(
        "Cannot book a time in the past"
    );

    public static Error ExceedsMaxDuration(int maxDurationMinutes) => Error.Validation(
        $"Booking cannot exceed {maxDurationMinutes} minute(s) for this resource type"
    );

    public static Error ExceedsMaxAdvance(int maxAdvanceDays) => Error.Validation(
        $"This resource cannot be booked more than {maxAdvanceDays} day(s) in advance"
    );

    public static Error OutsideAvailability() => Error.Validation(
        "This resource is not available for booking at the selected time"
    );

    public static Error ResourceUnavailable(ResourceStatus status) => Error.Conflict(
        $"This resource is not available for booking because its status is '{status}'."
    );

    public static readonly Error AlreadyCancelled = Error.Conflict(
        "Booking is already cancelled."
    );
}
