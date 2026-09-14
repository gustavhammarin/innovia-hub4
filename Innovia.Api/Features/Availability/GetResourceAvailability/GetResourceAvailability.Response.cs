namespace Innovia.Api.Features.Availability.GetResourceAvailability;

public sealed record Response(
    Guid ResourceId,
    IReadOnlyList<SlotResponse> Slots
);

public sealed record SlotResponse(
    DateTimeOffset StartUtc,
    DateTimeOffset EndUtc,
    bool IsAvailable
);
