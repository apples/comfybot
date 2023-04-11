using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComfyBot.Migrations
{
    /// <inheritdoc />
    public partial class AdjustedSomeFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RemindMinutesBefore",
                table: "ScheduledEvents");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "Reminder",
                table: "ScheduledEvents",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AlterColumn<ulong>(
                name: "ReminderChannel",
                table: "GuildSettings",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(ulong),
                oldType: "INTEGER");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledEvents_GuildId",
                table: "ScheduledEvents",
                column: "GuildId");

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduledEvents_GuildSettings_GuildId",
                table: "ScheduledEvents",
                column: "GuildId",
                principalTable: "GuildSettings",
                principalColumn: "GuildId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScheduledEvents_GuildSettings_GuildId",
                table: "ScheduledEvents");

            migrationBuilder.DropIndex(
                name: "IX_ScheduledEvents_GuildId",
                table: "ScheduledEvents");

            migrationBuilder.DropColumn(
                name: "Reminder",
                table: "ScheduledEvents");

            migrationBuilder.AddColumn<int>(
                name: "RemindMinutesBefore",
                table: "ScheduledEvents",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<ulong>(
                name: "ReminderChannel",
                table: "GuildSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0ul,
                oldClrType: typeof(ulong),
                oldType: "INTEGER",
                oldNullable: true);
        }
    }
}
