namespace Innovia.Api.Features.ResourceTypes.CreateResourceType;

public sealed record Response(
    Guid Id,
    string Name,
    DateTimeOffset CreatedAt,
    int MaxDurationMinutes,
    int MaxAdvanceDays
);
