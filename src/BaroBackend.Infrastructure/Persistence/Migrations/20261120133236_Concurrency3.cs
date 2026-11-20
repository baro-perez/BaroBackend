using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaroBackend.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Concurrency3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "db_revision_field_name",
                table: "players",
                newName: "db_revision");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "db_revision",
                table: "players",
                newName: "db_revision_field_name");
        }
    }
}
