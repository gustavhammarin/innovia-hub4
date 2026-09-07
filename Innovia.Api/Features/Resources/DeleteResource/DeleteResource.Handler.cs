using Innovia.Api.Common.Database;
using Innovia.Api.Common.Database.Entities;
using Innovia.Api.Common.Result;
using Microsoft.EntityFrameworkCore;

namespace Innovia.Api.Features.Resources.DeleteResource;

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
            .FirstOrDefaultAsync(r => r.Id == command.Id, ct);

        if (resource is null)
            return Result<Response>.Fail(ResourceErrors.NotFound);

        if (resource.Status == ResourceStatus.Archived)
            return Result<Response>.Ok(new Response(resource.Id, resource.Status));

        var now = DateTimeOffset.UtcNow;

        var hasActiveBookings = await _context.Bookings
            .AnyAsync(b =>
                b.ResourceId == command.Id &&
                b.CancelledAt == null &&
                b.EndsAt > now,
                ct);

        if (hasActiveBookings)
            return Result<Response>.Fail(ResourceErrors.HasActiveBookings);

        resource.Status = ResourceStatus.Archived;
        await _context.SaveChangesAsync(ct);

        return Result<Response>.Ok(new Response(resource.Id, resource.Status));
    }
}
