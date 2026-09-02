using Innovia.Api.Common.Database;
using Innovia.Api.Common.Database.Entities;
using Innovia.Api.Common.Result;
using Innovia.Api.Features.Availability.ManageAvailabilityRules;
using Microsoft.EntityFrameworkCore;

namespace Innovia.Api.Features.Availability.ManageAvailabilityRules.CreateAvailabilityRule;

public sealed class Handler
{
    private readonly AppDbContext _context;

    public Handler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<AvailabilityRuleResponse>> HandleAsync(Command command, CancellationToken ct)
    {
        var resourceTypeExists = await _context.ResourceTypes
            .AsNoTracking()
            .AnyAsync(resourceType => resourceType.Id == command.ResourceTypeId, ct);

        if (!resourceTypeExists)
            return Result<AvailabilityRuleResponse>.Fail(AvailabilityErrors.ResourceTypeNotFound(command.ResourceTypeId));

        var duplicateRuleExists = await _context.AvailabilityRules
            .AsNoTracking()
            .AnyAsync(rule =>
                rule.ResourceTypeId == command.ResourceTypeId
                && rule.DayOfWeek == command.DayOfWeek, ct);

        if (duplicateRuleExists)
            return Result<AvailabilityRuleResponse>.Fail(AvailabilityErrors.DuplicateRule(command.ResourceTypeId, command.DayOfWeek));

        var rule = new AvailabilityRule
        {
            Id = Guid.CreateVersion7(),
            ResourceTypeId = command.ResourceTypeId,
            DayOfWeek = command.DayOfWeek,
            OpensAt = command.OpensAt,
            ClosesAt = command.ClosesAt,
            SlotDurationMinutes = command.SlotDurationMinutes
        };

        await _context.AvailabilityRules.AddAsync(rule, ct);

        try
        {
            await _context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.IsUniqueViolation())
        {
            return Result<AvailabilityRuleResponse>.Fail(AvailabilityErrors.DuplicateRule(command.ResourceTypeId, command.DayOfWeek));
        }
        catch (DbUpdateException ex) when (ex.IsForeignKeyViolation())
        {
            return Result<AvailabilityRuleResponse>.Fail(AvailabilityErrors.ResourceTypeNotFound(command.ResourceTypeId));
        }

        return Result<AvailabilityRuleResponse>.Ok(new AvailabilityRuleResponse(
            rule.Id,
            rule.ResourceTypeId,
            rule.DayOfWeek,
            rule.OpensAt,
            rule.ClosesAt,
            rule.SlotDurationMinutes
        ));
    }
}
