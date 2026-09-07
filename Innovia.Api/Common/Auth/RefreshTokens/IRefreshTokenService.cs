using Innovia.Api.Common.Database.Entities;
using Innovia.Api.Common.Result;

namespace Innovia.Api.Common.Auth.RefreshTokens;

public interface IRefreshTokenService
{
    Task<(string RawToken, Guid Id)> IssueAsync(Guid userId, CancellationToken ct);
    Task<RefreshToken?> ValidateAsync(string rawToken, CancellationToken ct);
    Task RevokeAsync(Guid tokenId, Guid replacedByTokenId, CancellationToken ct);
    Task RevokeAsync(Guid tokenId, CancellationToken ct);
    Task RevokeAllForUserAsync(Guid userId, CancellationToken ct);
}