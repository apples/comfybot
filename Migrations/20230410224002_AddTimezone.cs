using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComfyBot.Migrations
{
    /// <inheritdoc />
    public partial class AddTimezone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Timezone",
                table: "GuildSettings",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Timezone",
                table: "GuildSettings");
        }
    }
}
