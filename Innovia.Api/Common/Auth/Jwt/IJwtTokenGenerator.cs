using Innovia.Api.Common.Database.Entities;

namespace Innovia.Api.Common.Auth.Jwt;

public interface IJwtTokenGenerator
{
    string GenerateAccessToken(ApplicationUser user, IList<string> roles);
}