namespace Innovia.Api.Common.Auth.Jwt;

public sealed record JwtSettings(
    string Issuer, 
    string Audience, 
    string Secret, 
    int AccessTokenExpirationMinutes
);