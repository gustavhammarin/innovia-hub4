using Innovia.Api.Common.Auth.Jwt;
using Innovia.Api.Common.Database;
using Innovia.Api.Common.Database.Entities;
using Innovia.Api.Common.Result;
using Microsoft.AspNetCore.Identity;

namespace Innovia.Api.Features.Auth.Login;

public sealed class Handler
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public Handler(UserManager<ApplicationUser> userManager, IJwtTokenGenerator jwtTokenGenerator)
    {
        _userManager = userManager;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<Result<string>> HandleAsync(Command cmd, CancellationToken ct)
    {
        var user = await _userManager.FindByEmailAsync(cmd.Email);
        if (user is null)
            return Result<string>.Fail(AuthErrors.InvalidCredentials());
        
        var passwordValid = await _userManager.CheckPasswordAsync(user, cmd.Password);
        if (!passwordValid)
            return Result<string>.Fail(AuthErrors.InvalidCredentials());
        
        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user, roles);

        return Result<string>.Ok(accessToken);
    }
}