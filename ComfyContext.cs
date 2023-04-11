using Microsoft.EntityFrameworkCore;
using NodaTime;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class ComfyContext : DbContext
{
    public DbSet<ScheduledEvent> ScheduledEvents { get; set; }
    public DbSet<GuildSettings> GuildSettings { get; set; }

    public string DbPath { get; }

    public ComfyContext()
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = System.IO.Path.Join(Environment.GetFolderPath(folder), "ComfyBot");
        System.IO.Directory.CreateDirectory(path);
        DbPath = System.IO.Path.Join(path, "data.db");
    }

    // The following configures EF to create a Sqlite database file in the
    // special "local" folder for your platform.
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options
            .UseSqlite($"Data Source={DbPath}");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder
            .Entity<ScheduledEvent>()
                .Navigation(x => x.GuildSettings).AutoInclude();
}

public class ScheduledEvent
{
    public ulong ScheduledEventId { get; set; }
    [ForeignKey(nameof(GuildSettings))] public ulong GuildId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public DateTimeOffset? Start { get; set; } = null;
    public DateTimeOffset? End { get; set; } = null;
    public RecurrenceKind Recurrence { get; set; } = RecurrenceKind.Once;
    public int RecurrenceValue { get; set; } = 0;
    public TimeSpan? Reminder { get; set; } = null;

    public GuildSettings? GuildSettings { get; set; }

    private DateTimeOffset? _memoGetNextStartParam;
    private DateTimeOffset? _memoGetNextStartResult;

    public DateTimeOffset? GetNextStart()
    {
        if (Start == null)
            return null;

        if (_memoGetNextStartParam == Start)
            return _memoGetNextStartResult;

        _memoGetNextStartParam = Start;
        _memoGetNextStartResult = GetNextStartNoMemo();

        return _memoGetNextStartResult;
    }

    private DateTimeOffset? GetNextStartNoMemo()
    {
        if (Start == null)
            return null;

        if (Start.Value - DateTimeOffset.Now > TimeSpan.Zero || Recurrence == RecurrenceKind.Once)
            return Start;

        var tz = GuildSettings?.Timezone != null ? DateTimeZoneProviders.Tzdb[GuildSettings.Timezone] : DateTimeZone.Utc;

        var userStart = ZonedDateTime.FromDateTimeOffset(Start.Value).WithZone(tz);

        switch (Recurrence)
        {
            case RecurrenceKind.Weekly:
                var diff = userStart - ZonedDateTime.FromDateTimeOffset(DateTimeOffset.UtcNow);
                return userStart.LocalDateTime.PlusWeeks((int)diff.TotalDays / 7 + 1).InZoneLeniently(tz).ToDateTimeOffset();
            default:
                throw new NotSupportedException();
        };
    }

    public enum RecurrenceKind
    {
        Once,
        Weekly,
    }
}

public class GuildSettings
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
    public ulong GuildId { get; set; }
    public ulong? ReminderChannel { get; set; }
    public string? Timezone { get; set; }
}
