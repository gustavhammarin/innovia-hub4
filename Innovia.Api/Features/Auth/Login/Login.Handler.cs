using Innovia.Api.Common.Auth.Jwt;
using Innovia.Api.Common.Auth.RefreshTokens;
using Innovia.Api.Common.Database.Entities;
using Innovia.Api.Common.Result;
using Microsoft.AspNetCore.Identity;

namespace Innovia.Api.Features.Auth.Login;

public sealed class Handler
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IRefreshTokenService _refreshTokenService;

    public Handler(UserManager<ApplicationUser> userManager, IJwtTokenGenerator jwtTokenGenerator, IRefreshTokenService refreshTokenService)
    {
        _userManager = userManager;
        _jwtTokenGenerator = jwtTokenGenerator;
        _refreshTokenService = refreshTokenService;
    }

    public async Task<Result<Response>> HandleAsync(Command cmd, CancellationToken ct)
    {
        var user = await _userManager.FindByEmailAsync(cmd.Email);
        if (user is null)
            return Result<Response>.Fail(AuthErrors.InvalidCredentials());
        
        var passwordValid = await _userManager.CheckPasswordAsync(user, cmd.Password);
        if (!passwordValid)
            return Result<Response>.Fail(AuthErrors.InvalidCredentials());
        
        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user, roles);

        var issued = await _refreshTokenService.IssueAsync(user.Id, ct);

        return Result<Response>.Ok(new Response(accessToken, issued.RawToken));
    }
}