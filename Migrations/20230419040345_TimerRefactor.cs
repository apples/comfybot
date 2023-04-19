using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComfyBot.Migrations
{
    /// <inheritdoc />
    public partial class TimerRefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "EndTime",
                table: "ScheduledEvents",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ReminderDuration",
                table: "ScheduledEvents",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "StartTime",
                table: "ScheduledEvents",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ScheduledEventOccurences",
                columns: table => new
                {
                    ScheduledEventOccurrenceId = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ScheduledEventId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    When = table.Column<long>(type: "INTEGER", nullable: false),
                    IsReminder = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledEventOccurences", x => x.ScheduledEventOccurrenceId);
                    table.ForeignKey(
                        name: "FK_ScheduledEventOccurences_ScheduledEvents_ScheduledEventId",
                        column: x => x.ScheduledEventId,
                        principalTable: "ScheduledEvents",
                        principalColumn: "ScheduledEventId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledEventOccurences_ScheduledEventId",
                table: "ScheduledEventOccurences",
                column: "ScheduledEventId");

            migrationBuilder.Sql(@"
            update ScheduledEvents
            set StartTime = unixepoch(Start),
                EndTime = unixepoch(End);
            update ScheduledEvents
            set ReminderDuration =
                case when Reminder is null or StartTime is null
                    then null
                    else (case when instr(Reminder, ""."") = 0
                        then (cast(substr(Reminder, 1, 2) as integer)*60*60 + cast(substr(Reminder, 4, 2) as integer)*60 + cast(substr(Reminder, 7, 2) as integer))
                        else (cast(substr(Reminder, 1, instr(Reminder, ""."") - 1) as integer)*24*60*60 + cast(substr(Reminder, 1+instr(Reminder, "".""), 2) as integer)*60*60 + cast(substr(Reminder, 4+instr(Reminder, "".""), 2) as integer)*60 + cast(substr(Reminder, 7+instr(Reminder, "".""), 2) as integer))
                    end)
                end;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScheduledEventOccurences");

            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "ScheduledEvents");

            migrationBuilder.DropColumn(
                name: "ReminderDuration",
                table: "ScheduledEvents");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "ScheduledEvents");
        }
    }
}
