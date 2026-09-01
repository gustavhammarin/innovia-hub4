using Innovia.Api.Common.Contracts;
using Innovia.Api.Common.Database;
using Innovia.Api.Common.Errors;
using Innovia.Api.Common.Result;
using Innovia.Api.Features.Bookings;
using Microsoft.EntityFrameworkCore;

namespace Innovia.Api.Features.Bookings.UpdateBooking;

public sealed class Handler(AppDbContext dbContext)
{
    public async Task<Result<Response>> HandleAsync(Command command, CancellationToken ct)
    {
        var booking = await dbContext.Bookings
            .FirstOrDefaultAsync(x => x.Id == command.BookingId, ct);

        if (booking is null)
        {
            return Result<Response>.Fail(Error.NotFound("Booking was not found."));
        }

        if (booking.UserId != command.UserId)
        {
            return Result<Response>.Fail(Error.Forbidden("You cannot update this booking."));
        }

        var hasOverlap = await dbContext.Bookings.AnyAsync(x =>
            x.Id != command.BookingId &&
            x.ResourceId == command.ResourceId &&
            x.StartsAt < command.EndsAt &&
            command.StartsAt < x.EndsAt,
            ct);

        if (hasOverlap)
        {
            return Result<Response>.Fail(Error.Conflict("Resource is already booked for this time."));
        }

        booking.ResourceId = command.ResourceId;
        booking.StartsAt = command.StartsAt;
        booking.EndsAt = command.EndsAt;

        try
        {
            await dbContext.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.IsOverlapViolation())
        {
            return Result<Response>.Fail(BookingErrors.Overlaps);
        }
        catch (DbUpdateException ex) when (ex.IsForeignKeyViolation())
        {
            return Result<Response>.Fail(BookingErrors.InvalidReference());
        }

        var resource = await dbContext.Resources
            .AsNoTracking()
            .Select(r => new { r.Id, r.Name })
            .FirstAsync(r => r.Id == booking.ResourceId, ct);

        var user = await dbContext.Users
            .AsNoTracking()
            .Select(u => new { u.Id, u.Email })
            .FirstAsync(u => u.Id == booking.UserId, ct);

        return Result<Response>.Ok(new Response(
            booking.Id,
            new UserRef(user.Id, user.Email ?? "Unknown"),
            new ResourceRef(resource.Id, resource.Name),
            booking.StartsAt,
            booking.EndsAt,
            booking.CreatedAt,
            DateTime.Now
        ));
    }
}
