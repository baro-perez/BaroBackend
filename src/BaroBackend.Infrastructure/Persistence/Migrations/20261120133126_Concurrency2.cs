using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaroBackend.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Concurrency2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "row_version",
                table: "players");

            migrationBuilder.AddColumn<long>(
                name: "db_revision_field_name",
                table: "players",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "db_revision_field_name",
                table: "players");

            migrationBuilder.AddColumn<byte[]>(
                name: "row_version",
                table: "players",
                type: "bytea",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);
        }
    }
}
