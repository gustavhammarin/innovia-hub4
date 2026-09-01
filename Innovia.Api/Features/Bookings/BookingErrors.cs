using Innovia.Api.Common.Errors;

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
}