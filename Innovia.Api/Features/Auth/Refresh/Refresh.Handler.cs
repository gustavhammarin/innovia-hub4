using Innovia.Api.Common.Auth.Jwt;
using Innovia.Api.Common.Auth.RefreshTokens;
using Innovia.Api.Common.Database.Entities;
using Innovia.Api.Common.Result;
using Microsoft.AspNetCore.Identity;

namespace Innovia.Api.Features.Auth.Refresh;

public sealed class Handler
{
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    public Handler(IRefreshTokenService refreshTokenService, UserManager<ApplicationUser> userManager, IJwtTokenGenerator jwtTokenGenerator)
    {
        _refreshTokenService = refreshTokenService;
        _userManager = userManager;
        _jwtTokenGenerator = jwtTokenGenerator;
    }
    public async Task<Result<Response>> HandleAsync(Command command, CancellationToken ct)
    {
        var token = await _refreshTokenService.ValidateAsync(command.RawToken, ct);
        if (token is null)
            return Result<Response>.Fail(AuthErrors.InvalidCredentials());
        
        if (token.RevokedAt is not null){
            await _refreshTokenService.RevokeAllForUserAsync(token.UserId, ct);
            return Result<Response>.Fail(AuthErrors.InvalidCredentials());
        }

        var issued = await _refreshTokenService.IssueAsync(token.UserId, ct);
        await _refreshTokenService.RevokeAsync(token.Id, issued.Id, ct);

        var user = await _userManager.FindByIdAsync(token.UserId.ToString());
        if (user is null)
            return Result<Response>.Fail(AuthErrors.InvalidCredentials());

        var roles = await _userManager.GetRolesAsync(user);
        var newAccessToken = _jwtTokenGenerator.GenerateAccessToken(user, roles);


        return Result<Response>.Ok(new Response(newAccessToken, issued.RawToken));
    }
}