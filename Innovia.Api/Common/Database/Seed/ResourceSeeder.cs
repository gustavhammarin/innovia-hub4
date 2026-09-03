using Innovia.Api.Common.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Innovia.Api.Common.Database.Seed;

public sealed class ResourceSeeder
{
    private sealed record ResourceDefinition(string Name, string Description);

    private static readonly Dictionary<string, ResourceDefinition[]> ResourcesByTypeName = new()
    {
        ["Drop-in skrivbord"] = Enumerable.Range(1, 15)
            .Select(i => new ResourceDefinition($"Drop-in skrivbord {i}", "Bokningsbart drop-in-skrivbord."))
            .ToArray(),
        ["Mötesrum"] = Enumerable.Range(1, 4)
            .Select(i => new ResourceDefinition($"Mötesrum {i}", "Bokningsbart mötesrum."))
            .ToArray(),
        ["VR headset"] = Enumerable.Range(1, 4)
            .Select(i => new ResourceDefinition($"VR headset {i}", "Bokningsbart VR-headset."))
            .ToArray(),
        ["AI-server"] =
        [
            new ResourceDefinition("AI-server", "Bokningsbar AI-server med GPU-kapacitet.")
        ]
    };

    private readonly AppDbContext _context;

    public ResourceSeeder(AppDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        var resourceTypesByName = await _context.ResourceTypes
            .AsNoTracking()
            .ToDictionaryAsync(rt => rt.Name, rt => rt.Id);

        var existingResources = await _context.Resources.ToListAsync();

        var desiredNames = ResourcesByTypeName
            .Where(kvp => resourceTypesByName.ContainsKey(kvp.Key))
            .SelectMany(kvp => kvp.Value.Select(definition => definition.Name))
            .ToHashSet();

        var stale = existingResources.Where(resource => !desiredNames.Contains(resource.Name)).ToList();
        if (stale.Count > 0)
            _context.Resources.RemoveRange(stale);

        var existingNames = existingResources.Select(resource => resource.Name).ToHashSet();

        var toAdd = new List<Resource>();
        foreach (var (typeName, definitions) in ResourcesByTypeName)
        {
            if (!resourceTypesByName.TryGetValue(typeName, out var resourceTypeId))
                continue;

            foreach (var definition in definitions)
            {
                if (existingNames.Contains(definition.Name))
                    continue;

                toAdd.Add(new Resource
                {
                    Id = Guid.CreateVersion7(),
                    Name = definition.Name,
                    Description = definition.Description,
                    ResourceTypeId = resourceTypeId,
                    CreatedAt = DateTimeOffset.UtcNow
                });
            }
        }

        if (toAdd.Count > 0)
            await _context.Resources.AddRangeAsync(toAdd);

        if (stale.Count > 0 || toAdd.Count > 0)
            await _context.SaveChangesAsync();
    }
}
