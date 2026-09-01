namespace Innovia.Api.Features.Bookings.CancelBooking;

public sealed record Response(
    Guid Id,
    DateTimeOffset? CancelledAt

);