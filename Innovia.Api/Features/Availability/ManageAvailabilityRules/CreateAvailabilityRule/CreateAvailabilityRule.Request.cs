namespace Innovia.Api.Features.Availability.ManageAvailabilityRules.CreateAvailabilityRule;

public sealed record Request(
    Guid ResourceTypeId,
    DayOfWeek DayOfWeek,
    TimeOnly OpensAt,
    TimeOnly ClosesAt,
    int SlotDurationMinutes
);
