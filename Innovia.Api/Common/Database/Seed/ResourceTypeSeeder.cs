using Innovia.Api.Common.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Innovia.Api.Common.Database.Seed;

public sealed class ResourceTypeSeeder
{
    private static readonly string[] DesiredResourceTypeNames =
    [
        "Drop-in skrivbord",
        "Mötesrum",
        "VR headset",
        "AI-server"
    ];

    private readonly AppDbContext _context;

    public ResourceTypeSeeder(AppDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        var existing = await _context.ResourceTypes.ToListAsync();

        var stale = existing.Where(rt => !DesiredResourceTypeNames.Contains(rt.Name)).ToList();
        if (stale.Count > 0)
            _context.ResourceTypes.RemoveRange(stale);

        var existingNames = existing.Select(rt => rt.Name).ToHashSet();
        var toAdd = DesiredResourceTypeNames
            .Where(name => !existingNames.Contains(name))
            .Select(name => new ResourceType
            {
                Id = Guid.CreateVersion7(),
                Name = name,
                CreatedAt = DateTimeOffset.UtcNow
            })
            .ToList();

        if (toAdd.Count > 0)
            await _context.ResourceTypes.AddRangeAsync(toAdd);

        if (stale.Count > 0 || toAdd.Count > 0)
            await _context.SaveChangesAsync();
    }
}
