using Innovia.Api.Common.Database;
using Innovia.Api.Common.Result;
using Microsoft.EntityFrameworkCore;

namespace Innovia.Api.Features.ResourceTypes.UpdateResourceType;

public sealed class Handler
{
    private readonly AppDbContext _context;

    public Handler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Response>> HandleAsync(Command command, CancellationToken ct)
    {
        var resourceType = await _context.ResourceTypes
            .FirstOrDefaultAsync(rt => rt.Id == command.Id, ct);

        if (resourceType is null)
            return Result<Response>.Fail(ResourceTypeErrors.NotFound);

        var name = command.Name.Trim();
        var normalizedName = name.ToLower();

        var duplicateNameExists = await _context.ResourceTypes
            .AsNoTracking()
            .AnyAsync(rt => rt.Id != command.Id && rt.Name.ToLower() == normalizedName, ct);

        if (duplicateNameExists)
            return Result<Response>.Fail(ResourceTypeErrors.DuplicateName(name));

        resourceType.Name = name;
        resourceType.MaxDurationMinutes = command.MaxDurationMinutes;
        resourceType.MaxAdvanceDays = command.MaxAdvanceDays;

        await _context.SaveChangesAsync(ct);

        return Result<Response>.Ok(new Response(
            resourceType.Id,
            resourceType.Name,
            resourceType.MaxDurationMinutes,
            resourceType.MaxAdvanceDays
        ));
    }
}
