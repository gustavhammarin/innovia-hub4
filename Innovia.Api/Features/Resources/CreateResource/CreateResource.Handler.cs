using Innovia.Api.Common.Database;
using Innovia.Api.Common.Database.Entities;
using Innovia.Api.Common.Result;
using Innovia.Api.Features.ResourceTypes;
using Microsoft.EntityFrameworkCore;

namespace Innovia.Api.Features.Resources.CreateResource;

public sealed class Handler
{
    private readonly AppDbContext _context;

    public Handler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Response>> HandleAsync(Command command, CancellationToken ct)
    {
        var resourceTypeExists = await _context.ResourceTypes
            .AnyAsync(resourceType => resourceType.Id == command.ResourceTypeId, ct);

        if (!resourceTypeExists)
            return Result<Response>.Fail(ResourceTypeErrors.NotFound);

        var resource = new Resource
        {
            Id = Guid.CreateVersion7(),
            Name = command.Name.Trim(),
            Description = command.Description.Trim(),
            ResourceTypeId = command.ResourceTypeId,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _context.Resources.AddAsync(resource, ct);
        await _context.SaveChangesAsync(ct);

        return Result<Response>.Ok(new Response(
            resource.Id,
            resource.Name,
            resource.Description,
            resource.ResourceTypeId,
            resource.CreatedAt,
            resource.Status
        ));
    }
}
