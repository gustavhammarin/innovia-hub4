using Innovia.Api.Common.Contracts;

namespace Innovia.Api.Features.Bookings.GetBookingById;

public sealed record Response (
    Guid Id,
    UserRef User,
    ResourceRef Resource,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    DateTimeOffset CreatedAt
);