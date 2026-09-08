namespace Innovia.Api.Features.Occupancy.GetOccupancyByRange;

public sealed record Command(DateOnly FromDate, DateOnly ToDate);