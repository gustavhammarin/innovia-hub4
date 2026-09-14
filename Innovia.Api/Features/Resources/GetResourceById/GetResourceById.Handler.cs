using Innovia.Api.Common.Database;
using Innovia.Api.Common.Database.Entities;
using Innovia.Api.Common.Result;
using Microsoft.EntityFrameworkCore;

namespace Innovia.Api.Features.Resources.GetResourceById;

public sealed class Handler
{
    private readonly AppDbContext _context;

    public Handler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Response>> HandleAsync(Command command, CancellationToken ct)
    {
        var resource = await _context.Resources
            .AsNoTracking()
            .Where(r => r.Id == command.Id && r.Status != ResourceStatus.Archived)
            .Select(r => new Response(
                r.Id,
                r.Name,
                r.Description,
                r.ResourceTypeId,
                r.CreatedAt,
                r.Status))
            .FirstOrDefaultAsync(ct);

        return resource is null
            ? Result<Response>.Fail(ResourceErrors.NotFound)
            : Result<Response>.Ok(resource);
    }
}
