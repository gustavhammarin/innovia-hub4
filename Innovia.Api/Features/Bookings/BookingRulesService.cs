using Innovia.Api.Common.Database;
using Innovia.Api.Common.Errors;
using Innovia.Api.Common.Time;
using Microsoft.EntityFrameworkCore;

namespace Innovia.Api.Features.Bookings;

public sealed class BookingRulesService
{
    private readonly AppDbContext _context;
    public BookingRulesService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Error?> ValidateAsync(Guid resourceId, DateTimeOffset startsAt, DateTimeOffset endsAt, CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;

        if (startsAt < now)
            return BookingErrors.CannotBookInPast;

        var localStart = TimeZoneInfo.ConvertTime(startsAt, SwedenTimeZone.Instance);
        var localDate = DateOnly.FromDateTime(localStart.DateTime);
        var dayOfWeek = localStart.DayOfWeek;

        var data = await _context.Resources
            .AsNoTracking()
            .Where(r => r.Id == resourceId)
            .Join(_context.ResourceTypes, r => r.ResourceTypeId, rt => rt.Id, (r, rt) => rt)
            .Select(rt => new
            {
                ResourceType = rt,
                Rule = _context.AvailabilityRules
                    .Where(ar => ar.ResourceTypeId == rt.Id && ar.DayOfWeek == dayOfWeek)
                    .FirstOrDefault()
            })
            .FirstOrDefaultAsync(ct);

        if (data is null)
            return null;

        var resourceType = data.ResourceType;

        var bookingDuration = endsAt - startsAt;
        var maxDuration = TimeSpan.FromMinutes(resourceType.MaxDurationMinutes);
        if (bookingDuration > maxDuration)
            return BookingErrors.ExceedsMaxDuration(resourceType.MaxDurationMinutes);

        if (startsAt > now.AddDays(resourceType.MaxAdvanceDays))
            return BookingErrors.ExceedsMaxAdvance(resourceType.MaxAdvanceDays);

        if (data.Rule is null)
            return BookingErrors.OutsideAvailability();

        var openUtc = SwedenTimeZone.ToUtc(localDate, data.Rule.OpensAt);
        var closeUtc = SwedenTimeZone.ToUtc(localDate, data.Rule.ClosesAt);

        if (startsAt < openUtc || endsAt > closeUtc)
            return BookingErrors.OutsideAvailability();

        return null;
    }
}
