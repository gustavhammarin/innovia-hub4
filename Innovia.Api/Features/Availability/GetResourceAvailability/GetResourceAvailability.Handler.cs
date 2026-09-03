using Innovia.Api.Common.Database;
using Innovia.Api.Common.Time;
using Innovia.Api.Common.Result;
using Microsoft.EntityFrameworkCore;

namespace Innovia.Api.Features.Availability.GetResourceAvailability;

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
            .Where(resource => resource.Id == command.ResourceId)
            .Select(resource => new { resource.Id, resource.ResourceTypeId })
            .FirstOrDefaultAsync(ct);

        if (resource is null)
            return Result<Response>.Fail(AvailabilityErrors.ResourceNotFound(command.ResourceId));

        var rules = await _context.AvailabilityRules
            .AsNoTracking()
            .Where(rule => rule.ResourceTypeId == resource.ResourceTypeId)
            .ToDictionaryAsync(rule => rule.DayOfWeek, ct);

        var slotCandidates = new List<(DateTimeOffset StartUtc, DateTimeOffset EndUtc)>();

        for (var date = command.FromDate; date <= command.ToDate; date = date.AddDays(1))
        {
            if (!rules.TryGetValue(date.DayOfWeek, out var rule))
                continue;

            var slotStartLocal = rule.OpensAt;

            while (slotStartLocal.AddMinutes(rule.SlotDurationMinutes) <= rule.ClosesAt)
            {
                var slotEndLocal = slotStartLocal.AddMinutes(rule.SlotDurationMinutes);

                slotCandidates.Add((
                    SwedenTimeZone.ToUtc(date, slotStartLocal),
                    SwedenTimeZone.ToUtc(date, slotEndLocal)
                ));

                slotStartLocal = slotEndLocal;
            }
        }

        if (slotCandidates.Count == 0)
            return Result<Response>.Ok(new Response(resource.Id, []));

        var startsAt = slotCandidates.Min(slot => slot.StartUtc);
        var endsAt = slotCandidates.Max(slot => slot.EndUtc);

        var bookings = await _context.Bookings
            .AsNoTracking()
            .Where(booking =>
                booking.ResourceId == command.ResourceId
                && booking.CancelledAt == null
                && booking.StartsAt < endsAt
                && booking.EndsAt > startsAt)
            .Select(booking => new { booking.StartsAt, booking.EndsAt })
            .ToListAsync(ct);

        var slots = slotCandidates
            .Select(slot => new SlotResponse(
                slot.StartUtc,
                slot.EndUtc,
                !bookings.Any(booking =>
                    booking.StartsAt < slot.EndUtc
                    && booking.EndsAt > slot.StartUtc)))
            .ToList();

        return Result<Response>.Ok(new Response(resource.Id, slots));
    }
}
