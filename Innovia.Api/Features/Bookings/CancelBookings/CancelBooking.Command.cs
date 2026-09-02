namespace Innovia.Api.Features.Bookings.CancelBooking;

public sealed record Command(
    Guid BookingId,
    Guid UserId,
    bool IsAdmin
);