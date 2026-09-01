using Innovia.Api.Common.Contracts;
using Innovia.Api.Common.Database;
using Innovia.Api.Common.Result;
using Microsoft.EntityFrameworkCore;

namespace Innovia.Api.Features.Bookings.GetBookingById;

public sealed class Handler
{
    private readonly AppDbContext _context;

    public Handler (AppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Response>> HandleAsync (Command command, CancellationToken ct)
    {
        var booking = await _context.Bookings
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == command.BookingId, ct);

        var resource = await _context.Resources
            .AsNoTracking()
            .Select(r => new { r.Id, r.Name })
            .FirstAsync(r => r.Id == booking.ResourceId, ct);

        var user = await _context.Users
            .AsNoTracking()
            .Select(u => new { u.Id, u.Email })
            .FirstAsync(u => u.Id == booking.UserId, ct);

        if (booking is null)
            return Result<Response>.Fail(BookingErrors.NotFound); //lade till ett Booking Error

        var resp = new Response(
            booking.Id,
            new UserRef(user.Id, user.Email ?? "Unknown"),
            new ResourceRef (resource.Id, resource.Name),
            booking.StartsAt,
            booking.EndsAt,
            booking.CreatedAt
        );

        return Result<Response>.Ok(resp);
    }
    
}