namespace Innovia.Api.Features.Availability.ManageAvailabilityRules.CreateAvailabilityRule;

public sealed record Command(
    Guid ResourceTypeId,
    DayOfWeek DayOfWeek,
    TimeOnly OpensAt,
    TimeOnly ClosesAt,
    int SlotDurationMinutes
);
