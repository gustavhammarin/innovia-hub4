namespace Innovia.Api.Common.Errors;

public sealed record ValidationError(string Field, string Message);