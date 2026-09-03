using Innovia.Api.Common.Database;
using Innovia.Api.Common.Result;
using Innovia.Api.Features.Availability.ManageAvailabilityRules;
using Microsoft.EntityFrameworkCore;

namespace Innovia.Api.Features.Availability.ManageAvailabilityRules.ListAvailabilityRules;

public sealed class Handler
{
    private readonly AppDbContext _context;

    public Handler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<AvailabilityRuleResponse>>> HandleAsync(Command command, CancellationToken ct)
    {
        var resourceTypeExists = await _context.ResourceTypes
            .AsNoTracking()
            .AnyAsync(resourceType => resourceType.Id == command.ResourceTypeId, ct);

        if (!resourceTypeExists)
            return Result<List<AvailabilityRuleResponse>>.Fail(AvailabilityErrors.ResourceTypeNotFound(command.ResourceTypeId));

        var rules = await _context.AvailabilityRules
            .AsNoTracking()
            .Where(rule => rule.ResourceTypeId == command.ResourceTypeId)
            .OrderBy(rule => rule.DayOfWeek)
            .Select(rule => new AvailabilityRuleResponse(
                rule.Id,
                rule.ResourceTypeId,
                rule.DayOfWeek,
                rule.OpensAt,
                rule.ClosesAt,
                rule.SlotDurationMinutes))
            .ToListAsync(ct);

        return Result<List<AvailabilityRuleResponse>>.Ok(rules);
    }
}
