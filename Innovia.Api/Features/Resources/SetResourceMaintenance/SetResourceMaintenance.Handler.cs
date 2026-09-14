using Innovia.Api.Common.Database;
using Innovia.Api.Common.Database.Entities;
using Innovia.Api.Common.Result;
using Innovia.Api.Features.Realtime;
using Microsoft.EntityFrameworkCore;

namespace Innovia.Api.Features.Resources.SetResourceMaintenance;

public sealed class Handler
{
    private readonly AppDbContext _context;
    private readonly IResourceNotifier _notifier;

    public Handler(AppDbContext context, IResourceNotifier notifier)
    {
        _context = context;
        _notifier = notifier;
    }

    public async Task<Result<Response>> HandleAsync(Command command, CancellationToken ct)
    {
        var resource = await _context.Resources
            .FirstOrDefaultAsync(r => r.Id == command.Id, ct);

        if (resource is null)
            return Result<Response>.Fail(ResourceErrors.NotFound);

        if (resource.Status == ResourceStatus.Archived)
            return Result<Response>.Fail(ResourceErrors.ArchivedCannotChangeStatus);

        resource.Status = ResourceStatus.Maintenance;
        await _context.SaveChangesAsync(ct);

        await _notifier.ResourceStatusChangedAsync(resource.Id, resource.Status, ct);

        return Result<Response>.Ok(new Response(resource.Id, resource.Status));
    }
}
