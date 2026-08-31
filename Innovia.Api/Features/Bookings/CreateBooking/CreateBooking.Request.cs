namespace Innovia.Api.Features.Bookings.CreateBooking;

public sealed record Request(
    Guid ResourceId, 
    DateTimeOffset StartsAt, 
    DateTimeOffset EndsAt
);