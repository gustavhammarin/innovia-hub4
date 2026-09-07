using Innovia.Api.Common.Database;
using Innovia.Api.Common.Result;
using Microsoft.EntityFrameworkCore;

namespace Innovia.Api.Features.Resources.GetResourceByIdAdmin;

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
            .Where(resource => resource.Id == command.Id)
            .Select(resource => new Response(
                resource.Id,
                resource.Name,
                resource.Description,
                resource.ResourceTypeId,
                resource.CreatedAt,
                resource.Status
            ))
            .FirstOrDefaultAsync(ct);

        return resource is null
            ? Result<Response>.Fail(ResourceErrors.NotFound)
            : Result<Response>.Ok(resource);
    }
}
