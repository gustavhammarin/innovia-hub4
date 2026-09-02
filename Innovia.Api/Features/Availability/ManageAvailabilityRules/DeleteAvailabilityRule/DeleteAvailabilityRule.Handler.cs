using Innovia.Api.Common.Database;
using Innovia.Api.Common.Result;
using Microsoft.EntityFrameworkCore;

namespace Innovia.Api.Features.Availability.ManageAvailabilityRules.DeleteAvailabilityRule;

public sealed class Handler
{
    private readonly AppDbContext _context;

    public Handler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Result> HandleAsync(Command command, CancellationToken ct)
    {
        var rule = await _context.AvailabilityRules
            .FirstOrDefaultAsync(rule => rule.Id == command.Id, ct);

        if (rule is null)
            return Result.Fail(AvailabilityErrors.RuleNotFound);

        _context.AvailabilityRules.Remove(rule);
        await _context.SaveChangesAsync(ct);

        return Result.Ok();
    }
}
