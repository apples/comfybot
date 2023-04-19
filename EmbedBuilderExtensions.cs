
using Discord;

public static class EmbedBuilderExtensions
{
    public static EmbedBuilder AddEvent(this EmbedBuilder embed, ScheduledEvent e, SwizzleService swizzler)
    {
        embed
            .AddField("Event", e.Name, true)
            .AddField("Id", $"`{swizzler.Swizzle(e.ScheduledEventId)}`", true)
            .AddEmptyField();

        embed
            .AddField("Start", e.StartTime.ToEmbedAsTimestamp(), true);
        if (e.EndTime != null)
            embed.AddField("End", e.EndTime.ToEmbedAsTimestamp(), true);
        else
            embed.AddEmptyField();
        embed.AddEmptyField();

        if (e.Recurrence == ScheduledEvent.RecurrenceKind.Once)
        {
            embed
                .AddField("Recurrence", "None", true)
                .AddEmptyField().AddEmptyField();
        }
        else
        {
            embed
                .AddField("Recurrence", e.Recurrence.ToString(), true)
                .AddField("Recurrence Interval", e.RecurrenceValue, true)
                .AddEmptyField();
        }

        if (e.ReminderDuration != null && e.StartTime != null)
            embed.AddField("Reminder", TimeSpan.FromSeconds(e.StartTime.Value - e.ReminderDuration.Value).ToString(), true);
        else
            embed.AddField("Reminder", "none", true);
        embed.AddEmptyField().AddEmptyField();

        embed
            .AddField("Description", e.Description, false);

        return embed;
    }

    public static EmbedBuilder AddEventShort(this EmbedBuilder embed, ScheduledEvent e)
    {
        embed
            .AddField("Event", e.Name, true)
            .AddEmptyField().AddEmptyField();

        embed.AddField("Start", e.StartTime.ToEmbedAsTimestamp(), true);
        if (e.EndTime != null)
            embed.AddField("End", e.EndTime.ToEmbedAsTimestamp(), true);
        else
            embed.AddEmptyField();

        embed.AddField("Description", e.Description, false);

        return embed;
    }

    public static EmbedBuilder AddEmptyField(this EmbedBuilder embed)
    {
        return embed.AddField("\u200B", "\u200B", true);
    }
}
