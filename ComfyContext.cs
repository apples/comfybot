using Microsoft.EntityFrameworkCore;
using NodaTime;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class ComfyContext : DbContext
{
    public DbSet<ScheduledEvent> ScheduledEvents { get; set; }
    public DbSet<ScheduledEventOccurrence> ScheduledEventOccurences { get; set; }
    public DbSet<GuildSettings> GuildSettings { get; set; }

    public string DbPath { get; }

    public ComfyContext()
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = System.IO.Path.Join(Environment.GetFolderPath(folder), "ComfyBot");
        if (!System.IO.Directory.Exists(path))
        {
            Console.WriteLine($"Creating database directory \"{path}\"");
            System.IO.Directory.CreateDirectory(path);
        }
        DbPath = System.IO.Path.Join(path, "data.db");
        Console.WriteLine($"Using database \"{DbPath}\"");
    }

    // The following configures EF to create a Sqlite database file in the
    // special "local" folder for your platform.
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options
            .UseSqlite($"Data Source={DbPath}");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<ScheduledEvent>()
                .Navigation(x => x.GuildSettings).AutoInclude();
    }
}

/// <summary>
/// All times are Unix second timestamps, durations are seconds.
/// </summary>
public class ScheduledEvent
{
    public ulong ScheduledEventId { get; set; }
    [ForeignKey(nameof(GuildSettings))] public ulong GuildId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public long? StartTime { get; set; } = null;
    public long? EndTime { get; set; } = null;
    public long? ReminderDuration { get; set; } = null;
    public RecurrenceKind Recurrence { get; set; } = RecurrenceKind.Once;
    public int RecurrenceValue { get; set; } = 0;

    public GuildSettings? GuildSettings { get; set; }

    private long? _memoGetNextStartParam;
    private long? _memoGetNextStartResult;

    public long? GetNextStart()
    {
        if (StartTime == null)
            return null;

        if (_memoGetNextStartParam == StartTime)
            return _memoGetNextStartResult;

        _memoGetNextStartParam = StartTime;
        _memoGetNextStartResult = GetNextStartNoMemo();

        return _memoGetNextStartResult;
    }

    private long? GetNextStartNoMemo()
    {
        if (StartTime == null)
            return null;

        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        if (StartTime.Value > now)
            return StartTime;
        
        if (Recurrence == RecurrenceKind.Once)
            return null;

        var start = DateTimeOffset.FromUnixTimeSeconds(StartTime.Value);

        var tz = GuildSettings?.Timezone != null ? DateTimeZoneProviders.Tzdb[GuildSettings.Timezone] : DateTimeZone.Utc;

        var userStart = ZonedDateTime.FromDateTimeOffset(start).WithZone(tz);

        switch (Recurrence)
        {
            case RecurrenceKind.Weekly:
                var diff = ZonedDateTime.FromDateTimeOffset(DateTimeOffset.UtcNow) - userStart;
                return userStart.LocalDateTime.PlusWeeks((int)diff.TotalDays / 7 + 1).InZoneLeniently(tz).ToDateTimeOffset().ToUnixTimeSeconds();
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

[Index(nameof(When))]
public class ScheduledEventOccurrence
{
    public ulong ScheduledEventOccurrenceId { get; set; }
    [ForeignKey(nameof(ScheduledEvent))] public ulong ScheduledEventId { get; set; }
    public long When { get; set; }
    public bool IsReminder { get; set; }

    public ScheduledEvent? ScheduledEvent { get; set; }
}
