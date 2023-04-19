
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public sealed class EventScheduler
{
    private IServiceProvider _services;
    private DiscordSocketClient _client;

    private Timer? _current_timer;
    private long _current_timer_due;

    public EventScheduler(IServiceProvider services, DiscordSocketClient client)
    {
        _services = services;
        _client = client;
    }

    public async Task InitializeAsync()
    {
        Console.WriteLine("Bootstrapping timers...");

        await using var scope = _services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ComfyContext>();

        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        await db.ScheduledEventOccurences.ExecuteDeleteAsync();

        var futureEvents = await db.ScheduledEvents.Where(e => e.Recurrence != ScheduledEvent.RecurrenceKind.Once || (e.StartTime - e.ReminderDuration) > now || e.StartTime > now).ToListAsync();

        foreach (var e in futureEvents)
        {
            InsertOccurrences(db, e, now);
        }

        await db.SaveChangesAsync();

        Console.WriteLine("Bootstrapping timers done.");
    }

    public async Task UpdateOccurrences(ComfyContext db, ScheduledEvent e)
    {
        var occurrences = await db.ScheduledEventOccurences.Where(o => o.ScheduledEventId == e.ScheduledEventId).ToListAsync();

        var nextStart = e.GetNextStart();

        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        Console.WriteLine($"{nextStart} {now}");

        if (nextStart == null)
        {
            db.RemoveRange(occurrences);
            return;
        }

        // reminders

        var reminderOccurrences = occurrences.Where(x => x.IsReminder).ToList();

        if (e.ReminderDuration != null && nextStart - e.ReminderDuration.Value > now)
        {
            db.RemoveRange(reminderOccurrences.Skip(1));

            var when = nextStart.Value - e.ReminderDuration.Value;

            if (reminderOccurrences.Count == 0)
            {
                var newOccurrence = new ScheduledEventOccurrence
                {
                    ScheduledEventId = e.ScheduledEventId,
                    When = when,
                    IsReminder = true,
                };

                db.Add(newOccurrence);
            }
            else
            {
                reminderOccurrences[0].When = when;
            }

            PokeTimer(when);
        }
        else
        {
            db.RemoveRange(reminderOccurrences);
        }

        // starts

        var startOccurrences = occurrences.Where(x => !x.IsReminder).ToList();

        db.RemoveRange(startOccurrences.Skip(1));

        if (startOccurrences.Count == 0)
        {
            var newOccurrence = new ScheduledEventOccurrence
            {
                ScheduledEventId = e.ScheduledEventId,
                When = nextStart.Value,
                IsReminder = false,
            };

            db.Add(newOccurrence);
        }
        else
        {
            startOccurrences[0].When = nextStart.Value;
        }

        e.StartTime = nextStart;

        PokeTimer(nextStart.Value);
    }

    public Task DeleteOccurrences(ComfyContext db, ScheduledEvent e)
    {
        throw new NotImplementedException();
    }

    private void InsertOccurrences(ComfyContext db, ScheduledEvent e, long now)
    {
        var nextStart = e.GetNextStart();

        if (nextStart == null)
            return;

        if (e.ReminderDuration != null)
        {
            var reminderTime = nextStart.Value - e.ReminderDuration.Value;

            if (reminderTime > now)
            {
                var occurrenceReminder = new ScheduledEventOccurrence
                {
                    ScheduledEventId = e.ScheduledEventId,
                    When = reminderTime,
                    IsReminder = true,
                };

                db.Add(occurrenceReminder);
            }
        }

        var occurrence = new ScheduledEventOccurrence
        {
            ScheduledEventId = e.ScheduledEventId,
            When = nextStart.Value,
            IsReminder = false,
        };

        db.Add(occurrence);

        e.StartTime = nextStart;
    }

    private void PokeTimer(long nextStart)
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        var duration = Math.Max(nextStart - now, 1);

        var due = now + duration;

        if (_current_timer == null)
        {
            _current_timer = new Timer(OnTimer);
            _current_timer.Change(TimeSpan.FromSeconds(duration), Timeout.InfiniteTimeSpan);
            _current_timer_due = due;
            Console.WriteLine($"New timer due at {_current_timer_due}");
            return;
        }

        if (_current_timer_due < due)
            return;

        _current_timer.Change(TimeSpan.FromSeconds(duration), Timeout.InfiniteTimeSpan);
        _current_timer_due = due;
        Console.WriteLine($"Updated timer due at {_current_timer_due}");
    }

    private async Task BumpTimer(ComfyContext db)
    {
        StopTimer();

        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        var nextOccurrence = await db.ScheduledEventOccurences.Where(x => x.When > now).OrderBy(x => x.When).FirstOrDefaultAsync();

        if (nextOccurrence != null)
            PokeTimer(nextOccurrence.When);
    }

    private void StopTimer()
    {
        if (_current_timer != null)
        {
            Console.WriteLine("Stopping timer");
            _current_timer.Dispose();
            _current_timer = null;
        }
    }

    private async void OnTimer(object? state)
    {
        try
        {
            await using var scope = _services.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<ComfyContext>();

            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            var nextOccurrence = await db.ScheduledEventOccurences.Where(x => x.When > now).OrderBy(x => x.When).FirstOrDefaultAsync();

            if (nextOccurrence != null)
                PokeTimer(nextOccurrence.When);

            var occurrences = await db.ScheduledEventOccurences.Where(x => x.When <= now).ToListAsync();

            foreach (var e in occurrences)
            {
                await ProcessTimer(e, db);
            }

            await db.SaveChangesAsync();

            await BumpTimer(db);
        }
        catch (Exception e)
        {
            Console.WriteLine($"An exception occurred while processing a timer: {e}");
        }
    }

    private async Task ProcessTimer(ScheduledEventOccurrence occurrence, ComfyContext db)
    {
        Console.WriteLine($"Timer firing for event {occurrence.ScheduledEventId}");

        var e = await db.ScheduledEvents.FindAsync(occurrence.ScheduledEventId);

        if (e == null)
            return;

        SocketTextChannel? channel = null;

        var guildSettings = e.GuildSettings;

        if (guildSettings != null && guildSettings.ReminderChannel != null)
        {
            var ichannel = await _client.GetChannelAsync(guildSettings.ReminderChannel.Value);

            if (ichannel is SocketTextChannel stc)
            {
                channel = stc;
            }
        }

        if (channel == null)
        {
            channel = _client.GetGuild(e.GuildId).DefaultChannel;
        }

        var embed = new EmbedBuilder();

        if (occurrence.IsReminder)
        {
            var startingIn = e.StartTime - occurrence.When;

            var days = startingIn / (24 * 60 * 60);
            var hours = startingIn / (60 * 60) % 24;
            var minutes = startingIn / 60 % 60;
            var seconds = startingIn % 60;

            List<string> seq = new(4);

            if (days > 0)
                seq.Add($"{days} days");

            if (hours > 0 || days > 0)
                seq.Add($"{hours} hours");

            if (minutes > 0 || ((hours > 0 || days > 0) && seconds > 0))
                seq.Add($"{minutes} minutes");

            if (seconds > 0)
                seq.Add($"{seconds} seconds");

            var str = "";

            if (seq.Count == 1)
            {
                str = seq[0];
            }
            else
            {
                for (var i = 0; i < seq.Count; ++i)
                {
                    if (i == seq.Count - 1)
                        str += $"and {seq[i]}";
                    else
                        str += $"{seq[i]}, ";
                }
            }

            if (str == "")
                str = $"{startingIn} seconds";

            embed.WithDescription($"**[Reminder]** Starting in {str}.")
                .AddEventShort(e);
        }
        else
        {
            embed.WithDescription($"**[ :sparkles: STARTING NOW :sparkles: ]**")
                .AddEventShort(e);
        }

        var message = await channel.SendMessageAsync("", embed: embed.Build());

        if (occurrence.IsReminder)
            await message.AddReactionsAsync(new Emoji[] { "🟢", "❌" });
        
        await UpdateOccurrences(db, e);
    }
}
