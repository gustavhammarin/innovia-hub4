using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Innovia.Api.Common.Database;

public static class PostgresExceptionExtensions
{
    public static bool IsOverlapViolation(this DbUpdateException ex) =>
        ex.InnerException is PostgresException {SqlState: PostgresErrorCodes.ExclusionViolation};

    public static bool IsForeignKeyViolation(this DbUpdateException ex) => 
        ex.InnerException is PostgresException {SqlState: PostgresErrorCodes.ForeignKeyViolation};

    public static bool IsUniqueViolation(this DbUpdateException ex) => 
        ex.InnerException is PostgresException {SqlState: PostgresErrorCodes.UniqueViolation};    
}