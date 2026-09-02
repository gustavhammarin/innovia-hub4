using Innovia.Api.Common.Auth;
using Innovia.Api.Common.Auth.Jwt;
using Innovia.Api.Common.Database.Entities;
using Innovia.Api.Common.Result;
using Microsoft.AspNetCore.Identity;

namespace Innovia.Api.Features.Auth.Register;

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
        var existingUser = await _userManager.FindByEmailAsync(cmd.Email);
        if (existingUser is not null)
            return Result<string>.Fail(AuthErrors.EmailAlreadyExists());

        var user = new ApplicationUser
        {
            UserName = cmd.Email,
            Email = cmd.Email
        };

        var createResult = await _userManager.CreateAsync(user, cmd.Password);
        if (!createResult.Succeeded)
        {
            var message = string.Join(" ", createResult.Errors.Select(e => e.Description));
            return Result<string>.Fail(AuthErrors.RegistrationFailed(message));
        }

        await _userManager.AddToRoleAsync(user, Roles.Member);

        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user, roles);

        return Result<string>.Ok(accessToken);
    }
}
