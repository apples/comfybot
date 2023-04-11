using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComfyBot.Migrations
{
    /// <inheritdoc />
    public partial class AddScheduledEventFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "ScheduledEvents",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "End",
                table: "ScheduledEvents",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Recurrence",
                table: "ScheduledEvents",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RecurrenceValue",
                table: "ScheduledEvents",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RemindMinutesBefore",
                table: "ScheduledEvents",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "Start",
                table: "ScheduledEvents",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "ScheduledEvents");

            migrationBuilder.DropColumn(
                name: "End",
                table: "ScheduledEvents");

            migrationBuilder.DropColumn(
                name: "Recurrence",
                table: "ScheduledEvents");

            migrationBuilder.DropColumn(
                name: "RecurrenceValue",
                table: "ScheduledEvents");

            migrationBuilder.DropColumn(
                name: "RemindMinutesBefore",
                table: "ScheduledEvents");

            migrationBuilder.DropColumn(
                name: "Start",
                table: "ScheduledEvents");
        }
    }
}
