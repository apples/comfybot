
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
            .AddField("Start", e.Start.ToEmbed(), true);
        if (e.End != null)
            embed.AddField("End", e.End.ToEmbed(), true);
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
                .AddField("Recurrence Interval", e.RecurrenceValue)
                .AddEmptyField();
        }

        embed
            .AddField("Reminder", e.Reminder == null ? "None" : e.Reminder.ToString(), true)
            .AddEmptyField().AddEmptyField();

        embed
            .AddField("Description", e.Description, false);

        return embed;
    }

    public static EmbedBuilder AddEventShort(this EmbedBuilder embed, ScheduledEvent e)
    {
        embed
            .AddField("Event", e.Name, true)
            .AddEmptyField().AddEmptyField();

        embed
            .AddField("Start", e.Start.ToEmbed(), true)
            .AddField("End", e.End.ToEmbed(), true)
            .AddEmptyField();

        embed
            .AddField("Description", e.Description, false);

        return embed;
    }

    public static EmbedBuilder AddEmptyField(this EmbedBuilder embed)
    {
        return embed.AddField("\u200B", "\u200B", true);
    }
}
