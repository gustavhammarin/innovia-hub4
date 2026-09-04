namespace Innovia.Api.Features.ResourceTypes.CreateResourceType;

public sealed record Request(
    string Name,
    int MaxDurationMinutes = 480,
    int MaxAdvanceDays = 90
);
