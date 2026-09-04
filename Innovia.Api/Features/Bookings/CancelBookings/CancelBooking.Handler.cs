using Innovia.Api.Common.Database;
using Innovia.Api.Common.Result;
using Innovia.Api.Features.Bookings;
using Microsoft.EntityFrameworkCore;

namespace Innovia.Api.Features.Bookings.CancelBooking;

public class Handler
{
    private readonly AppDbContext _dbContext;
    public Handler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<Result<Response>> HandleAsync(Command command, CancellationToken ct)
    {
        var booking = await _dbContext.Bookings.FirstOrDefaultAsync(b => b.Id == command.BookingId, ct)
    ;

        if (booking is null)
            return Result<Response>.Fail(BookingErrors.NotFound);

        if (booking.UserId != command.UserId && !command.IsAdmin)
            return Result<Response>.Fail(BookingErrors.NotAuthorizedToUpdate);

        if (booking.CancelledAt is not null)
            return Result<Response>.Fail(BookingErrors.AlreadyCancelled);

        booking.CancelledAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(ct);

        return Result<Response>.Ok(new Response(
            booking.Id,
            booking.CancelledAt
        ));

    }
}
