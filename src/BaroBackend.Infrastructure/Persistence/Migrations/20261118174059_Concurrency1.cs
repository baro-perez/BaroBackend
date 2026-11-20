using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaroBackend.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Concurrency1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "row_version",
                table: "players",
                type: "bytea",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "row_version",
                table: "players");
        }
    }
}
