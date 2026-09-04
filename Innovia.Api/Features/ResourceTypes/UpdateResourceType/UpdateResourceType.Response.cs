namespace Innovia.Api.Features.ResourceTypes.UpdateResourceType;

public sealed record Response(
    Guid Id,
    string Name,
    int MaxDurationMinutes,
    int MaxAdvanceDays
);
