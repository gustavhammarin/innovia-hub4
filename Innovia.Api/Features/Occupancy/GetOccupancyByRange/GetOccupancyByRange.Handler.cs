using Innovia.Api.Common.Database;
using Innovia.Api.Common.Database.Entities;
using Innovia.Api.Common.Result;
using Innovia.Api.Common.Time;
using Microsoft.EntityFrameworkCore;

namespace Innovia.Api.Features.Occupancy.GetOccupancyByRange;

public sealed class Handler
{
    private readonly AppDbContext _context;

    public Handler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Response>> HandleAsync(Command command, CancellationToken ct)
    {
        var rangeStartUtc = SwedenTimeZone.ToUtc(command.FromDate, TimeOnly.MinValue);
        var rangeEndUtc = SwedenTimeZone.ToUtc(command.ToDate.AddDays(1), TimeOnly.MinValue);

        var resources = await _context.Resources
            .AsNoTracking()
            .Where(r => r.Status == ResourceStatus.Online)
            .Select(r => new { r.Id, r.ResourceTypeId })
            .ToListAsync(ct);

        var resourceIds = resources.Select(r => r.Id).ToList();

        var rules = await _context.AvailabilityRules
            .AsNoTracking()
            .ToListAsync(ct);

        var bookings = await _context.Bookings
            .AsNoTracking()
            .Where(b =>
                resourceIds.Contains(b.ResourceId) &&
                b.CancelledAt == null &&
                b.StartsAt < rangeEndUtc &&
                b.EndsAt > rangeStartUtc)
            .Select(b => new { b.ResourceId, b.StartsAt, b.EndsAt })
            .ToListAsync(ct);

        var resourceTypeIds = resources.Select(r => r.ResourceTypeId).Distinct().ToList();
        var resourceTypes = await _context.ResourceTypes
            .AsNoTracking()
            .Where(rt => resourceTypeIds.Contains(rt.Id))
            .ToDictionaryAsync(rt => rt.Id, ct);

        var byResourceType = new List<ResourceTypeOccupancy>();
        var dayCount = command.ToDate.DayNumber - command.FromDate.DayNumber + 1;
        var allDates = Enumerable.Range(0, dayCount).Select(i => command.FromDate.AddDays(i)).ToList();

        foreach (var typeGroup in resources.GroupBy(r => r.ResourceTypeId))
        {
            var typeResourceIds = typeGroup.Select(r => r.Id).ToHashSet();
            var typeRuleByDay = rules
                .Where(r => r.ResourceTypeId == typeGroup.Key)
                .ToDictionary(r => r.DayOfWeek);

            double availableHours = 0;
            foreach (var date in allDates)
            {
                if (!typeRuleByDay.TryGetValue(date.DayOfWeek, out var rule))
                    continue;

                var opensUtc = SwedenTimeZone.ToUtc(date, rule.OpensAt);
                var closesUtc = SwedenTimeZone.ToUtc(date, rule.ClosesAt);
                availableHours += (closesUtc - opensUtc).TotalHours * typeGroup.Count();
            }

            var bookedHours = bookings
                .Where(b => typeResourceIds.Contains(b.ResourceId))
                .Sum(b =>
                {
                    var clippedStart = b.StartsAt < rangeStartUtc ? rangeStartUtc : b.StartsAt;
                    var clippedEnd = b.EndsAt > rangeEndUtc ? rangeEndUtc : b.EndsAt;
                    return Math.Max(0, (clippedEnd - clippedStart).TotalHours);
                });

            var name = resourceTypes.TryGetValue(typeGroup.Key, out var rt) ? rt.Name : "Okänd";
            var percentage = availableHours == 0 ? 0 : Math.Round(bookedHours * 100.0 / availableHours, 1);

            byResourceType.Add(new ResourceTypeOccupancy(typeGroup.Key, name, Math.Round(bookedHours, 1), Math.Round(availableHours, 1), percentage));
        }

        var totalAvailable = byResourceType.Sum(t => t.AvailableHours);
        var totalBooked = byResourceType.Sum(t => t.BookedHours);
        var totalPercentage = totalAvailable == 0 ? 0 : Math.Round(totalBooked * 100.0 / totalAvailable, 1);

        return Result<Response>.Ok(new Response(totalPercentage, byResourceType));
    }
}