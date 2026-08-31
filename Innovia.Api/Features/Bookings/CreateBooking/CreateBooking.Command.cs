namespace Innovia.Api.Features.Bookings.CreateBooking;

public sealed record Command(
    Guid ResourceId, 
    Guid UserId, 
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt
);