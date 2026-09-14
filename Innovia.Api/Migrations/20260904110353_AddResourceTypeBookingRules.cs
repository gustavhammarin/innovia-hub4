using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Innovia.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddResourceTypeBookingRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxAdvanceDays",
                table: "ResourceTypes",
                type: "integer",
                nullable: false,
                defaultValue: 90);

            migrationBuilder.AddColumn<int>(
                name: "MaxDurationMinutes",
                table: "ResourceTypes",
                type: "integer",
                nullable: false,
                defaultValue: 480);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxAdvanceDays",
                table: "ResourceTypes");

            migrationBuilder.DropColumn(
                name: "MaxDurationMinutes",
                table: "ResourceTypes");
        }
    }
}
