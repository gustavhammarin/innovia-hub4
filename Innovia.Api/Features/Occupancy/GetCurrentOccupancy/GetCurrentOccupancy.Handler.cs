using Innovia.Api.Common.Database;
using Innovia.Api.Common.Database.Entities;
using Innovia.Api.Common.Result;
using Microsoft.EntityFrameworkCore;

namespace Innovia.Api.Features.Occupancy.GetCurrentOccupancy;

public sealed class Handler
{
    private readonly AppDbContext _context;

    public Handler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Response>> HandleAsync(Command command, CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;

        var resources = await _context.Resources
            .AsNoTracking()
            .Where(r => r.Status == ResourceStatus.Online)
            .Select(r => new { r.Id, r.ResourceTypeId })
            .ToListAsync(ct);

        var resourceIds = resources.Select(r => r.Id).ToList();

        var bookedResourceIds = await _context.Bookings
            .AsNoTracking()
            .Where(b =>
                resourceIds.Contains(b.ResourceId) &&
                b.CancelledAt == null &&
                b.StartsAt <= now &&
                b.EndsAt > now)
            .Select(b => b.ResourceId)
            .Distinct()
            .ToListAsync(ct);

        var bookedSet = bookedResourceIds.ToHashSet();

        var resourceTypes = await _context.ResourceTypes
            .AsNoTracking()
            .Select(rt => new { rt.Id, rt.Name })
            .ToDictionaryAsync(rt => rt.Id, ct);

        var byResourceType = resources
            .GroupBy(r => r.ResourceTypeId)
            .Select(g =>
            {
                var bookedCount = g.Count(r => bookedSet.Contains(r.Id));
                var totalCount = g.Count();
                var percentage = totalCount == 0 ? 0 : Math.Round(bookedCount * 100.0 / totalCount, 1);
                return new ResourceTypeOccupancy(
                    g.Key,
                    resourceTypes.TryGetValue(g.Key, out var type) ? type.Name : "Okänd",
                    bookedCount,
                    totalCount,
                    percentage
                );
            })
            .ToList();

        var totalPercentage = resources.Count == 0
            ? 0
            : Math.Round(bookedSet.Count * 100.0 / resources.Count, 1);

        return Result<Response>.Ok(new Response(totalPercentage, byResourceType));
    }
}