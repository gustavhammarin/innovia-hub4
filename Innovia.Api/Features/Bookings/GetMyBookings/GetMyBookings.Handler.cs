using Innovia.Api.Common.Contracts;
using Innovia.Api.Common.Database;
using Innovia.Api.Common.Result;
using Microsoft.EntityFrameworkCore;

namespace Innovia.Api.Features.Bookings.GetMyBookings;

public sealed class Handler
{
    private readonly AppDbContext _context;

    public Handler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<BookingResponse>>> HandleAsync(Command command, CancellationToken ct)
    {
        var bookings = await _context.Bookings
            .AsNoTracking()
            .Where(b => b.UserId == command.UserId)
            .ToListAsync(ct);

        if (bookings.Count == 0)
            return Result<List<BookingResponse>>.Ok([]);

        var user = await _context.Users
            .AsNoTracking()
            .Select(u => new { u.Id, u.Email })
            .FirstAsync(u => u.Id == command.UserId, ct);

        var resourceIds = bookings.Select(b => b.ResourceId).Distinct().ToList();

        var resources = await _context.Resources
            .AsNoTracking()
            .Where(r => resourceIds.Contains(r.Id))
            .Select(r => new { r.Id, r.Name, r.Description })
            .ToDictionaryAsync(r => r.Id, ct);

        var responses = bookings.Select(b => new BookingResponse(
            b.Id,
            new UserRef(user.Id, user.Email ?? "Unknown"),
            new ResourceRef(resources[b.ResourceId].Id, resources[b.ResourceId].Name, resources[b.ResourceId].Description),
            b.StartsAt,
            b.EndsAt,
            b.CreatedAt,
            b.CancelledAt
        )).ToList();

        return Result<List<BookingResponse>>.Ok(responses);
    }
}
