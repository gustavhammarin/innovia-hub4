using Innovia.Api.Common.Database;
using Innovia.Api.Common.Database.Entities;
using Innovia.Api.Common.Result;
using Microsoft.EntityFrameworkCore;

namespace Innovia.Api.Features.ResourceTypes.CreateResourceType;

public sealed class Handler
{
    private readonly AppDbContext _context;

    public Handler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Response>> HandleAsync(Command command, CancellationToken ct)
    {
        var name = command.Name.Trim();
        var normalizedName = name.ToLower();

        var duplicateNameExists = await _context.ResourceTypes
            .AsNoTracking()
            .AnyAsync(resourceType => resourceType.Name.ToLower() == normalizedName, ct);

        if (duplicateNameExists)
            return Result<Response>.Fail(ResourceTypeErrors.DuplicateName(name));

        var resourceType = new ResourceType
        {
            Id = Guid.CreateVersion7(),
            Name = name,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _context.ResourceTypes.AddAsync(resourceType, ct);
        await _context.SaveChangesAsync(ct);

        return Result<Response>.Ok(new Response(
            resourceType.Id,
            resourceType.Name,
            resourceType.CreatedAt
        ));
    }
}
