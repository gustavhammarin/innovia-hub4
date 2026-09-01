using Innovia.Api.Common.Contracts;
using Innovia.Api.Common.Database;
using Innovia.Api.Common.Database.Entities;
using Innovia.Api.Common.Result;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Innovia.Api.Features.Bookings.CreateBooking;

public sealed class Handler
{
    private readonly AppDbContext _context;

    public Handler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Response>> HandleAsync(Command cmd, CancellationToken ct)
    {
        var booking = new Booking
        {
            Id = Guid.CreateVersion7(),
            ResourceId = cmd.ResourceId,
            UserId = cmd.UserId,
            StartsAt = cmd.StartsAt,
            EndsAt = cmd.EndsAt,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _context.Bookings.AddAsync(booking, ct);

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

        var resource = await _context.Resources
            .AsNoTracking()
            .Select(r => new {r.Id, r.Name, r.Description})
            .FirstAsync(r => r.Id == booking.ResourceId, ct);
        
        var user = await _context.Users
            .AsNoTracking()
            .Select(u => new {u.Id, u.Email})
            .FirstAsync(u => u.Id == booking.UserId, ct);
        
        var resp = new Response
        (
            booking.Id,
            new UserRef(user.Id, user.Email ?? "Unknown"),
            new ResourceRef(resource.Id, resource.Name, resource.Description),
            booking.StartsAt,
            booking.EndsAt,
            booking.CreatedAt
        );

        return Result<Response>.Ok(resp);
    }
}
