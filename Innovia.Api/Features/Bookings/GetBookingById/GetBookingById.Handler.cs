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

    public async Task<Result<BookingResponse>> HandleAsync (Command command, CancellationToken ct)
    {
        var booking = await _context.Bookings
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == command.BookingId, ct);
        
        if (booking is null)
            return Result<BookingResponse>.Fail(BookingErrors.NotFound);

        if (booking.UserId != command.UserId && !command.IsAdmin)
            return Result<BookingResponse>.Fail(BookingErrors.NotAuthorized);

        var resource = await _context.Resources
            .AsNoTracking()
            .Select(r => new { r.Id, r.Name, r.Description })
            .FirstAsync(r => r.Id == booking.ResourceId, ct);

        var user = await _context.Users
            .AsNoTracking()
            .Select(u => new { u.Id, u.Email })
            .FirstAsync(u => u.Id == booking.UserId, ct);

        var resp = new BookingResponse(
            booking.Id,
            new UserRef(user.Id, user.Email ?? "Unknown"),
            new ResourceRef (resource.Id, resource.Name, resource.Description),
            booking.StartsAt,
            booking.EndsAt,
            booking.CreatedAt
        );

        return Result<BookingResponse>.Ok(resp);
    }
    
}