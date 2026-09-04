namespace Innovia.Api.Features.ResourceTypes.ListResourceTypes;

public sealed record Response(
    Guid Id,
    string Name,
    DateTimeOffset CreatedAt,
    int MaxDurationMinutes,
    int MaxAdvanceDays
);
