namespace Innovia.Api.Features.Availability.ManageAvailabilityRules.UpdateAvailabilityRule;

public sealed record Command(
    Guid Id,
    Guid ResourceTypeId,
    DayOfWeek DayOfWeek,
    TimeOnly OpensAt,
    TimeOnly ClosesAt,
    int SlotDurationMinutes
);
