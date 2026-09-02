namespace Innovia.Api.Features.Bookings.UpdateBooking;

public sealed record Request(
    Guid ResourceId,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt
);
