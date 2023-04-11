using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using ChronicParser = Chronic.Core.Parser;

namespace ComfyBot.Modules;

[RequireOwner]
[Group(name: "event", description: "Manage events")]
public class ComfyEventModule : InteractionModuleBase<SocketInteractionContext>
{
    private ComfyContext _db;

    private EventScheduler _scheduler;

    private SwizzleService _swizzler;

    public ComfyEventModule(ComfyContext db, EventScheduler scheduler, SwizzleService swizzler)
    {
        _db = db;
        _scheduler = scheduler;
        _swizzler = swizzler;
    }

    [SlashCommand(name: "create", description: "Creates an event")]
    public async Task Create(string name, string description)
    {
        var e = new ScheduledEvent
        {
            GuildId = Context.Guild.Id,
            Name = name,
            Description = description,
        };

        _db.ScheduledEvents.Add(e);

        await _db.SaveChangesAsync();

        _scheduler.AddSchedule(e);

        var embed = new EmbedBuilder()
            .WithDescription("Created event!")
            .AddEvent(e, _swizzler);

        await RespondAsync("", embed: embed.Build());
    }

    [SlashCommand(name: "set-details", description: "Set the details of the event")]
    public async Task SetDetails(string id, string? name = null, string? description = null)
    {
        var e = await GetEvent(id);

        if (e == null)
        {
            await RespondAsync("Event not found.");
            return;
        }

        if (name != null)
            e.Name = name;
        
        if (description != null)
            e.Description = description;

        await _db.SaveChangesAsync();

        _scheduler.UpdateSchedule(e);

        var embed = new EmbedBuilder()
            .WithDescription("Updated details.")
            .AddEvent(e, _swizzler);

        await RespondAsync("", embed: embed.Build());
    }

    [SlashCommand(name: "set-when", description: "Set the next occurrence of the event")]
    public async Task SetWhen(string id, string start, string? end = null)
    {
        var e = await GetEvent(id);

        if (e == null)
        {
            await RespondAsync("Event not found.");
            return;
        }

        var settings = e.GuildSettings;

        var dtStart = ParseDate(start, settings?.Timezone);
        DateTimeOffset? dtEnd = end != null ? ParseDate(end, settings?.Timezone) : null;

        if (dtStart == null)
        {
            await RespondAsync("Invalid start time.");
            return;
        }

        if (!string.IsNullOrWhiteSpace(end) && dtEnd == null)
        {
            await RespondAsync("Invalid end time.");
            return;
        }

        e.Start = dtStart;
        e.End = dtEnd;

        await _db.SaveChangesAsync();

        _scheduler.UpdateSchedule(e);

        var embed = new EmbedBuilder()
            .WithDescription("Updated start and end time.")
            .AddEvent(e, _swizzler);

        await RespondAsync("", embed: embed.Build());
    }

    [SlashCommand(name: "set-recur", description: "Set the recurrence of the event")]
    public async Task SetRecur(string id, ScheduledEvent.RecurrenceKind recur, int interval = 1)
    {
        var e = await GetEvent(id);

        if (e == null)
        {
            await RespondAsync("Event not found.");
            return;
        }

        e.Recurrence = recur;
        e.RecurrenceValue = interval;

        await _db.SaveChangesAsync();

        _scheduler.UpdateSchedule(e);

        var embed = new EmbedBuilder()
            .WithDescription("Updated recurrence.")
            .AddEvent(e, _swizzler);

        await RespondAsync("", embed: embed.Build());
    }

    [SlashCommand(name: "set-remind", description: "Set how long before the event to send a reminder")]
    public async Task SetRemind(string id, string seconds)
    {
        var e = await GetEvent(id);

        if (e == null)
        {
            await RespondAsync("Event not found.");
            return;
        }

        int value = 0;

        if (!string.IsNullOrWhiteSpace(seconds))
        {
            var computed = new System.Data.DataTable().Compute(seconds, "");
            if (computed != DBNull.Value)
                value = (int)computed;
        }

        if (value == 0)
        {
            e.Reminder = null;
        }
        else
        {
            e.Reminder = TimeSpan.FromSeconds(value);
        }

        await _db.SaveChangesAsync();

        _scheduler.UpdateSchedule(e);

        var embed = new EmbedBuilder();

        if (value == 0)
        {
            embed.WithDescription("Cleared recurrence.");
        }
        else
        {
            embed.WithDescription("Updated recurrence.");
        }

        embed.AddEvent(e, _swizzler);

        await RespondAsync("", embed: embed.Build());
    }

    [SlashCommand(name: "delete", description: "Delete an event")]
    public async Task Delete(string id)
    {
        var e = await GetEvent(id);

        if (e == null)
        {
            await RespondAsync("Event not found.");
            return;
        }

        _scheduler.DeleteSchedule(e);

        _db.ScheduledEvents.Remove(e);

        await _db.SaveChangesAsync();

        var embed = new EmbedBuilder()
            .WithDescription("Deleted event.")
            .AddField("Event", e.Name, true)
            .AddField("Id", "`deleted`", true);

        await RespondAsync("", embed: embed.Build());
    }

    [SlashCommand(name: "list", description: "List events")]
    public async Task List(bool include_old = false)
    {
        var ids = "";
        var names = "";
        var starts = "";

        await foreach (var x in _db.ScheduledEvents)
        {
            if (x.GuildId != Context.Guild.Id)
                continue;
            
            if (!include_old && x.Start != null && x.GetNextStart() < DateTimeOffset.UtcNow)
                continue;

            ids += $"`{_swizzler.Swizzle(x.ScheduledEventId)}`\n";
            names += $"{x.Name}\n";
            starts += $"{x.Start.ToEmbed()}\n";
        }

        var embed = new EmbedBuilder();

        if (ids != "")
        {
            embed
                .WithDescription($"Listing {(include_old ? "all" : "upcoming")} events.")
                .AddField("Id", ids, true)
                .AddField("Name", names, true)
                .AddField("Start", starts, true);
        }
        else
        {
            embed.WithDescription(include_old ? "This server has no events." : "There are no upcoming events.");
        }

        await RespondAsync("", embed: embed.Build());
    }

    private async Task<ScheduledEvent?> GetEvent(string id)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;

            var e = await _db.ScheduledEvents.FindAsync(_swizzler.UnSwizzle(id));

            if (e == null || e.GuildId != Context.Guild.Id)
                return null;

            return e;
        }
        catch (Exception e)
        {
            return null;
        }
    }

    private DateTimeOffset? ParseDate(string str, string? timezone)
    {
        var tz = timezone != null ? TimeZoneInfo.FindSystemTimeZoneById(timezone) : TimeZoneInfo.Utc;

        var offset = tz.GetUtcOffset(DateTime.UtcNow);

        var parser = new ChronicParser(new Chronic.Core.Options
        {
            Clock = () => Clock(offset),
        });

        var parserResult = parser.Parse(str);

        if (parserResult == null)
            return null;

        var parsed = parserResult.Start;

        if (parsed == null)
            return null;

        var result = new DateTimeOffset(DateTime.SpecifyKind(parsed.Value - offset, DateTimeKind.Utc));

        return result;
    }

    private DateTime Clock(TimeSpan offset)
    {
        var now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        
        var userNow = now + offset;

        return userNow;
    }

}
