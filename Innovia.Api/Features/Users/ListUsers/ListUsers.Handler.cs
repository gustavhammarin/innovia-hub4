using Innovia.Api.Common.Auth;
using Innovia.Api.Common.Database;
using Innovia.Api.Common.Result;
using Microsoft.EntityFrameworkCore;

namespace Innovia.Api.Features.Users.ListUsers;

public sealed class Handler
{
    private const int MaxResults = 20;

    private readonly AppDbContext _context;

    public Handler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<UserResponse>>> HandleAsync(Command command, CancellationToken ct)
    {
        var memberUserIds = _context.UserRoles
            .Join(_context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => new { ur.UserId, r.Name })
            .Where(x => x.Name == Roles.Member)
            .Select(x => x.UserId);

        var query = _context.Users.AsNoTracking()
            .Where(u => memberUserIds.Contains(u.Id));

        if (!string.IsNullOrWhiteSpace(command.Search))
        {
            var search = command.Search.Trim();
            query = query.Where(u =>
                EF.Functions.ILike(u.FirstName + " " + u.LastName, $"%{search}%") ||
                EF.Functions.ILike(u.Email!, $"%{search}%"));
        }

        var users = await query
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .Take(MaxResults)
            .Select(u => new UserResponse(u.Id, u.FirstName + " " + u.LastName, u.Email ?? "Unknown"))
            .ToListAsync(ct);

        return Result<List<UserResponse>>.Ok(users);
    }
}
