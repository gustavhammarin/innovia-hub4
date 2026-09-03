using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Innovia.Api.Migrations
{
    /// <inheritdoc />
    public partial class FixOverlapConstraintExcludeCancelled : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                    ALTER TABLE ""Bookings""
                    DROP CONSTRAINT no_overlapping_bookings;
                ");

            migrationBuilder.Sql(@"
                    ALTER TABLE ""Bookings""
                    ADD CONSTRAINT no_overlapping_bookings
                    EXCLUDE USING gist (
                        ""ResourceId"" WITH =,
                        tstzrange(""StartsAt"", ""EndsAt"", '[)') WITH &&
                    )
                    WHERE (""CancelledAt"" IS NULL);
                ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                    ALTER TABLE ""Bookings""
                    DROP CONSTRAINT no_overlapping_bookings;
                ");

            migrationBuilder.Sql(@"
                    ALTER TABLE ""Bookings""
                    ADD CONSTRAINT no_overlapping_bookings
                    EXCLUDE USING gist (
                        ""ResourceId"" WITH =,
                        tstzrange(""StartsAt"", ""EndsAt"", '[)') WITH &&
                    );
                ");
        }
    }
}
