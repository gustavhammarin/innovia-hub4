namespace Innovia.Api.Common.Auth;

public interface ICurrentUser
{
    Guid? UserId {get;}
    bool IsAuthenticated {get;}
}