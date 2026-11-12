using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaroBackend.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PlayerNameLength : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "ck_players_name_length",
                table: "players",
                sql: "char_length(name) between 3 and 32");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_players_name_length",
                table: "players");
        }
    }
}
