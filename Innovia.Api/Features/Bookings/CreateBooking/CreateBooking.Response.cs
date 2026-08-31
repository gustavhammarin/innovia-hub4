using Innovia.Api.Common.Contracts;

namespace Innovia.Api.Features.Bookings.CreateBooking;

public sealed record Response(
    Guid BookingId,
    UserRef User,
    ResourceRef Resource,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    DateTimeOffset CreateAt
);



