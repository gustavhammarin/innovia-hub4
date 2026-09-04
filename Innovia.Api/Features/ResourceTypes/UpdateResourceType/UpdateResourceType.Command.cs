namespace Innovia.Api.Features.ResourceTypes.UpdateResourceType;

public sealed record Command(
    Guid Id,
    string Name,
    int MaxDurationMinutes,
    int MaxAdvanceDays
);
