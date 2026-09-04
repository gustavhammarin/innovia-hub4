using Innovia.Api.Common.Database;
using Innovia.Api.Common.Result;
using Microsoft.EntityFrameworkCore;

namespace Innovia.Api.Features.ResourceTypes.ListResourceTypes;

public sealed class Handler
{
    private readonly AppDbContext _context;

    public Handler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<Response>>> HandleAsync(Command command, CancellationToken ct)
    {
        var resourceTypes = await _context.ResourceTypes
            .AsNoTracking()
            .OrderBy(resourceType => resourceType.Name)
            .Select(resourceType => new Response(
                resourceType.Id,
                resourceType.Name,
                resourceType.CreatedAt,
                resourceType.MaxDurationMinutes,
                resourceType.MaxAdvanceDays))
            .ToListAsync(ct);

        return Result<List<Response>>.Ok(resourceTypes);
    }
}
