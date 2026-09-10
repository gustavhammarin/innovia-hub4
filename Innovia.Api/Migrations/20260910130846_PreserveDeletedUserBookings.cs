using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Innovia.Api.Migrations
{
    /// <inheritdoc />
    public partial class PreserveDeletedUserBookings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_AspNetUsers_UserId",
                table: "Bookings");

            migrationBuilder.AddColumn<bool>(
                name: "UserDeleted",
                table: "Bookings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UserEmailSnapshot",
                table: "Bookings",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserNameSnapshot",
                table: "Bookings",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("UPDATE \"Bookings\" b SET \"UserNameSnapshot\" = COALESCE(u.\"FirstName\", '') || CASE WHEN u.\"LastName\" IS NULL THEN '' ELSE ' ' || u.\"LastName\" END, \"UserEmailSnapshot\" = COALESCE(u.\"Email\", 'Unknown') FROM \"AspNetUsers\" u WHERE u.\"Id\" = b.\"UserId\"");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserDeleted",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "UserEmailSnapshot",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "UserNameSnapshot",
                table: "Bookings");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_AspNetUsers_UserId",
                table: "Bookings",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
