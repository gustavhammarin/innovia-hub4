using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Innovia.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddResourceStatusToResources : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Resources",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Resources");
        }
    }
}
