namespace Innovia.Api.Features.Availability.GetResourceAvailability;

public sealed record Command(Guid ResourceId, DateOnly FromDate, DateOnly ToDate);
