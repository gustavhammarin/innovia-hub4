using Innovia.Api.Common.Database;
using Innovia.Api.Common.Result;
using Microsoft.EntityFrameworkCore;

namespace Innovia.Api.Features.Resources.ListResourcesAdmin;

public sealed class Handler
{
    private readonly AppDbContext _context;

    public Handler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<Response>>> HandleAsync(CancellationToken ct)
    {
        var resources = await _context.Resources
            .AsNoTracking()
            .OrderBy(resource => resource.Name)
            .Select(resource => new Response(
                resource.Id,
                resource.Name,
                resource.Description,
                resource.ResourceTypeId,
                resource.CreatedAt,
                resource.Status
            ))
            .ToListAsync(ct);

        return Result<List<Response>>.Ok(resources);
    }
}
