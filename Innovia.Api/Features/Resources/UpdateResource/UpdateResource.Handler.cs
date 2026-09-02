using Innovia.Api.Common.Database;
using Innovia.Api.Common.Result;
using Microsoft.EntityFrameworkCore;

namespace Innovia.Api.Features.Resources.UpdateResource;

public sealed class Handler
{
    private readonly AppDbContext _context;

    public Handler (AppDbContext context)
    {
        _context = context;
    }

    public async Task <Result<Response>> HandleAsync (Command command, CancellationToken ct)
    {
        var resource = await _context.Resources
            .FirstOrDefaultAsync(r => r.Id == command.Id, ct);

        if (resource is null)
            return Result<Response>.Fail(ResourceErrors.NotFound); 
        
        resource.Name = command.Name;
        resource.ResourceTypeId = command.ResourceTypeId;

        try
        {
            await _context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.IsForeignKeyViolation())
        {
            return Result<Response>.Fail(ResourceErrors.InvalidReference());
        }

        var response = new Response(resource.Id, resource.Name, resource.ResourceTypeId);

        return Result<Response>.Ok(response);
    }
}