using Innovia.Api.Common.Contracts;
using Innovia.Api.Common.Database;
using Innovia.Api.Common.Result;
using Innovia.Api.Features.Bookings;
using Microsoft.EntityFrameworkCore;

namespace Innovia.Api.Features.Bookings.UpdateBooking;

public sealed class Handler
{
    private readonly AppDbContext _context;
    private readonly BookingRulesService _bookingRulesService;
    private readonly IBookingNotifier _notifier;

    public Handler(AppDbContext context, BookingRulesService bookingRulesService, IBookingNotifier notifier)
    {
        _context = context;
        _bookingRulesService = bookingRulesService;
        _notifier = notifier;
    }
    public async Task<Result<Response>> HandleAsync(Command command, CancellationToken ct)
    {

        var ruleViolationError = await _bookingRulesService.ValidateAsync(command.ResourceId, command.StartsAt, command.EndsAt, ct);
        if (ruleViolationError is not null)
            return Result<Response>.Fail(ruleViolationError);
            
        var booking = await _context.Bookings
            .FirstOrDefaultAsync(x => x.Id == command.BookingId, ct);

        if (booking is null)
        {
            return Result<Response>.Fail(BookingErrors.NotFound);
        }

        if (booking.UserId != command.UserId && !command.IsAdmin)
        {
            return Result<Response>.Fail(BookingErrors.NotAuthorizedToUpdate);
        }

        var oldResourceId = booking.ResourceId;

        booking.ResourceId = command.ResourceId;
        booking.StartsAt = command.StartsAt;
        booking.EndsAt = command.EndsAt;

        try
        {
            await _context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.IsOverlapViolation())
        {
            return Result<Response>.Fail(BookingErrors.Overlaps);
        }
        catch (DbUpdateException ex) when (ex.IsForeignKeyViolation())
        {
            return Result<Response>.Fail(BookingErrors.InvalidReference());
        }

        await _notifier.BookingUpdatedAsync(oldResourceId, booking.ResourceId, ct);

        var resource = await _context.Resources
            .AsNoTracking()
            .Select(r => new { r.Id, r.Name, r.Description })
            .FirstAsync(r => r.Id == booking.ResourceId, ct);

        var user = await _context.Users
            .AsNoTracking()
            .Select(u => new { u.Id, u.Email })
            .FirstAsync(u => u.Id == booking.UserId, ct);

        return Result<Response>.Ok(new Response(
            booking.Id,
            new UserRef(user.Id, user.Email ?? "Unknown"),
            new ResourceRef(resource.Id, resource.Name, resource.Description),
            booking.StartsAt,
            booking.EndsAt,
            booking.CreatedAt,
            DateTime.Now
        ));
    }
}
