using Innovia.Api.Common.Database;
using Innovia.Api.Common.Result;
using Innovia.Api.Features.Availability.ManageAvailabilityRules;
using Microsoft.EntityFrameworkCore;

namespace Innovia.Api.Features.Availability.ManageAvailabilityRules.UpdateAvailabilityRule;

public sealed class Handler
{
    private readonly AppDbContext _context;

    public Handler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<AvailabilityRuleResponse>> HandleAsync(Command command, CancellationToken ct)
    {
        var rule = await _context.AvailabilityRules
            .FirstOrDefaultAsync(rule => rule.Id == command.Id, ct);

        if (rule is null)
            return Result<AvailabilityRuleResponse>.Fail(AvailabilityErrors.RuleNotFound);

        var resourceTypeExists = await _context.ResourceTypes
            .AsNoTracking()
            .AnyAsync(resourceType => resourceType.Id == command.ResourceTypeId, ct);

        if (!resourceTypeExists)
            return Result<AvailabilityRuleResponse>.Fail(AvailabilityErrors.ResourceTypeNotFound(command.ResourceTypeId));

        var duplicateRuleExists = await _context.AvailabilityRules
            .AsNoTracking()
            .AnyAsync(otherRule =>
                otherRule.Id != command.Id
                && otherRule.ResourceTypeId == command.ResourceTypeId
                && otherRule.DayOfWeek == command.DayOfWeek, ct);

        if (duplicateRuleExists)
            return Result<AvailabilityRuleResponse>.Fail(AvailabilityErrors.DuplicateRule(command.ResourceTypeId, command.DayOfWeek));

        rule.ResourceTypeId = command.ResourceTypeId;
        rule.DayOfWeek = command.DayOfWeek;
        rule.OpensAt = command.OpensAt;
        rule.ClosesAt = command.ClosesAt;
        rule.SlotDurationMinutes = command.SlotDurationMinutes;

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
