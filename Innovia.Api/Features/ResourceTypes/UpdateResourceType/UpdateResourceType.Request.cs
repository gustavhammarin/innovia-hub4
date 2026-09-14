namespace Innovia.Api.Features.ResourceTypes.UpdateResourceType;

public sealed record Request(
    string Name,
    int MaxDurationMinutes,
    int MaxAdvanceDays
);
