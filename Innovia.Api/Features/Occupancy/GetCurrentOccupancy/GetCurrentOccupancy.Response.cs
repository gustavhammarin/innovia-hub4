namespace Innovia.Api.Features.Occupancy.GetCurrentOccupancy;

public sealed record Response(double TotalPercentage, List<ResourceTypeOccupancy> ByResourceType);

public sealed record ResourceTypeOccupancy(Guid ResourceTypeId, string Name, int BookedCount, int TotalCount);