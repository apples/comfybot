using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComfyBot.Migrations
{
    /// <inheritdoc />
    public partial class TimerRefactorCleanup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "End",
                table: "ScheduledEvents");

            migrationBuilder.DropColumn(
                name: "Reminder",
                table: "ScheduledEvents");

            migrationBuilder.DropColumn(
                name: "Start",
                table: "ScheduledEvents");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "End",
                table: "ScheduledEvents",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "Reminder",
                table: "ScheduledEvents",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "Start",
                table: "ScheduledEvents",
                type: "TEXT",
                nullable: true);

            migrationBuilder.Sql(@"
            update ScheduledEvents
            set Start = datetime(StartTime, ""unixepoch""),
                End = datetime(EndTime, ""unixepoch"");
            update ScheduledEvents
            set Reminder = case when StartTime is null
                then null
                else format(""%02d.%02d:%02d:%02d"",
                    (ReminderDuration / (24*60*60),
                    (ReminderDuration / (60*60)) % 24,
                    (ReminderDuration / 60) % 60,
                    (ReminderDuration % 60))
                end;
            ");
        }
    }
}
