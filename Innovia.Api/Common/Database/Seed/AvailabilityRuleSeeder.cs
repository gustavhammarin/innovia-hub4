using Innovia.Api.Common.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Innovia.Api.Common.Database.Seed;

public sealed class AvailabilityRuleSeeder
{
    private static readonly DayOfWeek[] DefaultOpenDays =
    [
        DayOfWeek.Monday,
        DayOfWeek.Tuesday,
        DayOfWeek.Wednesday,
        DayOfWeek.Thursday,
        DayOfWeek.Friday
    ];

    private readonly AppDbContext _context;

    public AvailabilityRuleSeeder(AppDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        var resourceTypeIds = await _context.ResourceTypes
            .AsNoTracking()
            .Select(resourceType => resourceType.Id)
            .ToListAsync();

        if (resourceTypeIds.Count == 0)
            return;

        var existingRules = await _context.AvailabilityRules
            .AsNoTracking()
            .Select(rule => new { rule.ResourceTypeId, rule.DayOfWeek })
            .ToListAsync();

        var existingRuleKeys = existingRules
            .Select(rule => (rule.ResourceTypeId, rule.DayOfWeek))
            .ToHashSet();

        var rulesToAdd = new List<AvailabilityRule>();

        foreach (var resourceTypeId in resourceTypeIds)
        {
            foreach (var dayOfWeek in DefaultOpenDays)
            {
                if (existingRuleKeys.Contains((resourceTypeId, dayOfWeek)))
                    continue;

                rulesToAdd.Add(new AvailabilityRule
                {
                    Id = Guid.CreateVersion7(),
                    ResourceTypeId = resourceTypeId,
                    DayOfWeek = dayOfWeek,
                    OpensAt = new TimeOnly(8, 0),
                    ClosesAt = new TimeOnly(16, 0),
                    SlotDurationMinutes = 60
                });
            }
        }

        if (rulesToAdd.Count == 0)
            return;

        await _context.AvailabilityRules.AddRangeAsync(rulesToAdd);
        await _context.SaveChangesAsync();
    }
}
