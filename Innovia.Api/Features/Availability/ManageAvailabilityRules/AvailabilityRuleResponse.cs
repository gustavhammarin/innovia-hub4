namespace Innovia.Api.Features.Availability.ManageAvailabilityRules;

public sealed record AvailabilityRuleResponse(
    Guid Id,
    Guid ResourceTypeId,
    DayOfWeek DayOfWeek,
    TimeOnly OpensAt,
    TimeOnly ClosesAt,
    int SlotDurationMinutes
);
