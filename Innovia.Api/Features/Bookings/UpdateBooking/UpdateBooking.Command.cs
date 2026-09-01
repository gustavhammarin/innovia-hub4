namespace Innovia.Api.Features.Bookings.UpdateBooking;

public sealed record Command(
    Guid BookingId,
    Guid ResourceId,
    Guid UserId,
    bool IsAdmin,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt
);
