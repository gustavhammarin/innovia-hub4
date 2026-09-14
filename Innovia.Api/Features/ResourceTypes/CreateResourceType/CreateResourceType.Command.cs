namespace Innovia.Api.Features.ResourceTypes.CreateResourceType;

public sealed record Command(
    string Name,
    int MaxDurationMinutes,
    int MaxAdvanceDays
);
