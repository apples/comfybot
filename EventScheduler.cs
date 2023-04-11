
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public sealed class EventScheduler : IDisposable
{
    private Dictionary<ulong, Timer> _timers = new();

    private object _lock = new();

    private IServiceProvider _services;
    private DiscordSocketClient _client;

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

        await foreach (var e in db.ScheduledEvents)
        {
            CreateTimer(e);
        }

        Console.WriteLine("Bootstrapping timers done.");
    }

    public void Dispose()
    {
        lock (_lock)
        {
            foreach (var x in _timers)
            {
                x.Value.Dispose();
            }

            _timers.Clear();
        }
    }

    public void UpdateSchedule(ScheduledEvent e)
    {
        if (e.Start == null)
        {
            DeleteSchedule(e);
        }
        else
        {
            CreateTimer(e);
        }
    }

    public void AddSchedule(ScheduledEvent e)
    {
        CreateTimer(e);
    }

    public void DeleteSchedule(ScheduledEvent e)
    {
        EraseTimer(e.ScheduledEventId);
    }

    private async void OnTimer(object? state)
    {
        try
        {
            var timerInfo = state as TimerInfo;

            if (timerInfo == null)
                return;

            EraseTimer(timerInfo.ScheduledEventId);

            await ProcessTimer(timerInfo);
        }
        catch (Exception e)
        {
            Console.WriteLine($"An exception occurred while processing a timer: {e}");
        }
    }

    private async Task ProcessTimer(TimerInfo timerInfo)
    {
        Console.WriteLine($"Timer firing for event {timerInfo.ScheduledEventId}");

        ScheduledEvent? e;
        await using (var scope = _services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ComfyContext>();
            e = await db.ScheduledEvents.FindAsync(timerInfo.ScheduledEventId);
        }

        if (e == null || e.GetNextStart() == null)
            return;

        if (timerInfo.IsReminder)
        {
            CreateStartTimer(e);
        }
        else if (e.Recurrence != ScheduledEvent.RecurrenceKind.Once)
        {
            CreateTimer(e);
        }

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

        if (timerInfo.IsReminder)
        {
            var startingIn = TimeSpan.FromSeconds(Math.Round((e.GetNextStart().Value - DateTimeOffset.Now).TotalSeconds));

            List<Tuple<int, string>> seq = new(4);

            var hasDay = startingIn.TotalDays > 0;
            var hasHour = startingIn.Hours > 0 || hasDay;
            var hasMin = startingIn.Minutes > 0;
            var hasSec = startingIn.Seconds > 0;

            if (hasDay)
            {
                seq.Add(new((int)startingIn.TotalDays, "days"));
            }

            if (hasHour)
            {
                seq.Add(new(startingIn.Hours, startingIn.Hours == 1 ? "hour" : "hours"));
            }

            if (hasMin || (hasHour && hasSec))
            {
                seq.Add(new(startingIn.Minutes, "minutes"));
            }

            if (hasSec)
            {
                seq.Add(new(startingIn.Seconds, "seconds"));
            }

            var str = "";

            for (var i = 0; i < seq.Count; ++i)
            {
                if (i == seq.Count - 1)
                    str += $"and {seq[i].Item1} {seq[i].Item2}";
                else
                    str += $"{seq[i].Item1} {seq[i].Item2} ";
            }

            if (str == "")
                str = $"{(int)startingIn.TotalSeconds} seconds";

            embed.WithDescription($"**[Reminder]** Starting in {str}.")
                .AddEventShort(e);
        }
        else
        {
            embed.WithDescription($"**[ :sparkles: STARTING NOW :sparkles: ]**")
                .AddEventShort(e);
        }

        var message = await channel.SendMessageAsync("", embed: embed.Build());

        if (timerInfo.IsReminder)
            await message.AddReactionsAsync(new Emoji[] { "🟢", "❌" });
    }

    private void CreateTimer(ScheduledEvent e)
    {
        var start = e.GetNextStart();

        if (start == null)
            return;
        
        if (e.Reminder == null)
        {
            CreateStartTimer(e);
            return;
        }

        var reminderTime = start.Value - e.Reminder.Value;

        var duration = reminderTime - DateTimeOffset.Now;

        if (duration < TimeSpan.Zero)
        {
            CreateStartTimer(e);
            return;
        }

        var timerInfo = new TimerInfo
        {
            ScheduledEventId = e.ScheduledEventId,
            IsReminder = true,
        };

        lock (_lock)
        {
            EraseTimer(e.ScheduledEventId);
            _timers[e.ScheduledEventId] = new Timer(OnTimer, timerInfo, duration, Timeout.InfiniteTimeSpan);
        }
    }

    private void CreateStartTimer(ScheduledEvent e)
    {
        var start = e.GetNextStart();

        if (start == null)
            return;
        
        var duration = start.Value - DateTimeOffset.Now;

        if (duration < TimeSpan.Zero)
            return;

        var timerInfo = new TimerInfo
        {
            ScheduledEventId = e.ScheduledEventId,
            IsReminder = false,
        };

        lock (_lock)
        {
            EraseTimer(e.ScheduledEventId);
            _timers[e.ScheduledEventId] = new Timer(OnTimer, timerInfo, duration, Timeout.InfiniteTimeSpan);
        }
    }

    private void EraseTimer(ulong scheduledEventId)
    {
        lock (_lock)
        {
            if (_timers.TryGetValue(scheduledEventId, out var timer))
            {
                timer.Dispose();
                _timers.Remove(scheduledEventId);
            }
        }
    }

    private class TimerInfo
    {
        public ulong ScheduledEventId { get; set; }
        public bool IsReminder { get; set; }
    }
}
