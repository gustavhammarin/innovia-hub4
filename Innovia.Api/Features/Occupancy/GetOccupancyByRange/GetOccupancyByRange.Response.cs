namespace Innovia.Api.Features.Occupancy.GetOccupancyByRange;

public sealed record Response(double TotalPercentage, List<ResourceTypeOccupancy> ByResourceType);

public sealed record ResourceTypeOccupancy(
    Guid ResourceTypeId,
    string Name,
    double BookedHours,
    double AvailableHours,
    double Percentage
);