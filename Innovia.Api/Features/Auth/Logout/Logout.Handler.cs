using Innovia.Api.Common.Auth.RefreshTokens;

namespace Innovia.Api.Features.Auth.Logout;

public sealed class Handler
{
    private readonly IRefreshTokenService _refreshTokenService;
    public Handler(IRefreshTokenService refreshTokenService)
    {
        _refreshTokenService = refreshTokenService;
    }

    public async Task HandleAsync(string refreshToken, CancellationToken ct)
    {
        var validated = await _refreshTokenService.ValidateAsync(refreshToken, ct);
        if (validated is null)
            return;

        if (validated.RevokedAt is not null){
            await _refreshTokenService.RevokeAllForUserAsync(validated.UserId, ct);
            return;
        }
        
        await _refreshTokenService.RevokeAsync(validated.Id, ct);
    }
}