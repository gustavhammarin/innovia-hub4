namespace Innovia.Api.Common.Auth;

public static class AuthorizationPolicies
{
    public const string AdminOnly = "AdminOnly";
    public const string MemberOnly = "MemberOnly";
    public const string MemberOrAdmin = "MemberOrAdmin";
}