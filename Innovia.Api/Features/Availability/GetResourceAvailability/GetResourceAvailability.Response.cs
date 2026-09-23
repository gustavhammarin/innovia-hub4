namespace Innovia.Api.Features.Availability.GetResourceAvailability;

public sealed record Response(
    Guid ResourceId,
    IReadOnlyList<SlotResponse> Slots,
    bool ClosedForRestOfToday
);

public sealed record SlotResponse(
    DateTimeOffset StartUtc,
    DateTimeOffset EndUtc,
    bool IsAvailable
);
