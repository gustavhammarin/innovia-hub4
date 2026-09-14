using Innovia.Api.Common.Auth.Jwt;
using Innovia.Api.Common.Database;
using Innovia.Api.Common.Database.Entities;
using Innovia.Api.Common.Errors;
using Innovia.Api.Common.Result;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Innovia.Api.Common.Auth.RefreshTokens;

public sealed class RefreshTokenService : IRefreshTokenService
{
    private readonly AppDbContext _context;
    private readonly JwtSettings _jwtSettings;
    public RefreshTokenService(AppDbContext context, IOptions<JwtSettings> jwtSettings)
    {
        _context = context;
        _jwtSettings = jwtSettings.Value;
    }
    public async Task<(string RawToken, Guid Id)> IssueAsync(Guid userId, CancellationToken ct)
    {
        var rawToken = RefreshTokenGenerator.Generate();
        var refreshToken = new RefreshToken
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
            TokenHash = RefreshTokenHasher.Hash(rawToken)
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync(ct);
        return (rawToken, refreshToken.Id);
    }

    public async Task RevokeAllForUserAsync(Guid userId, CancellationToken ct)
    {
        var refreshTokens = await _context.RefreshTokens.Where(x => x.UserId == userId && x.RevokedAt == null)
            .ExecuteUpdateAsync(x => x.SetProperty(x => x.RevokedAt, DateTimeOffset.UtcNow), ct);
    }

    public async Task RevokeAsync(Guid tokenId, Guid replacedByTokenId, CancellationToken ct)
    {
        await _context.RefreshTokens.Where(x => x.Id == tokenId && x.RevokedAt == null)
            .ExecuteUpdateAsync(x => x
                .SetProperty(x => x.RevokedAt, DateTimeOffset.UtcNow)
                .SetProperty(x => x.RevokedByTokenId, replacedByTokenId), ct);
    }

    public async Task RevokeAsync(Guid tokenId, CancellationToken ct)
    {
        await _context.RefreshTokens.Where(x => x.Id == tokenId && x.RevokedAt == null)
            .ExecuteUpdateAsync(x => x
                .SetProperty(x => x.RevokedAt, DateTimeOffset.UtcNow), ct);
    }

    public async Task<RefreshToken?> ValidateAsync(string rawToken, CancellationToken ct)
    {
        var tokenHash = RefreshTokenHasher.Hash(rawToken);
        var token = await _context.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == tokenHash, ct);
        if (token is null || token.ExpiresAt < DateTimeOffset.UtcNow)
            return null;

        return token;
    }
}