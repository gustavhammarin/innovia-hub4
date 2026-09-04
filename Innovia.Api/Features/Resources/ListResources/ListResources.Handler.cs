using Innovia.Api.Common.Database;
using Innovia.Api.Common.Database.Entities;
using Innovia.Api.Common.Result;
using Microsoft.EntityFrameworkCore;

namespace Innovia.Api.Features.Resources.ListResources;

public sealed class Handler
{
    private readonly AppDbContext _dbContext;
    public Handler (AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<Result<List<Response>>> HandleAsync(CancellationToken cancellationToken)
    {
        var resources = await _dbContext.Resources
            .AsNoTracking()
            .Where(resource => resource.Status != ResourceStatus.Archived)
            .OrderBy(resource => resource.Name)
            .Select(r => new Response(            
                r.Id,
                r.Name,
                r.Description,
                r.ResourceTypeId,
                r.CreatedAt,
                r.Status
            ))
            .ToListAsync(cancellationToken);

        return Result<List<Response>>.Ok(resources);
    }
}
