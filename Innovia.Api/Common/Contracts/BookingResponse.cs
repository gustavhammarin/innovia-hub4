namespace Innovia.Api.Common.Contracts;

public sealed record BookingResponse(
    Guid Id,
    UserRef User,
    ResourceRef Resource,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CancelledAt
);
