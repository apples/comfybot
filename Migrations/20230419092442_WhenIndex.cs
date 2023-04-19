using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComfyBot.Migrations
{
    /// <inheritdoc />
    public partial class WhenIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ScheduledEventOccurences_When",
                table: "ScheduledEventOccurences",
                column: "When");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ScheduledEventOccurences_When",
                table: "ScheduledEventOccurences");
        }
    }
}
