using Discord;
using Discord.Interactions;

namespace ComfyBot.Modules;

[RequireOwner]
[Group(name: "settings", description: "Manage settings")]
public class ComfySettingsModule : InteractionModuleBase<SocketInteractionContext>
{
    private ComfyContext _db;

    public ComfySettingsModule(ComfyContext db)
    {
        _db = db;
    }

    [SlashCommand(name: "set-reminder-channel", description: "Set the channel to send event reminders to.")]
    public async Task SetReminderChannel(IChannel channel)
    {
        var settings = await GetSettings();

        settings.ReminderChannel = channel.Id;

        await _db.SaveChangesAsync();

        await RespondAsync($"Set reminder channel: {channel.Name}.");
    }

    [SlashCommand(name: "set-timezone", description: "Set the timezone event dates will be specified in.")]
    public async Task SetTimezone([Autocomplete(typeof(TimezoneAutocompleteHandler))] string timezone)
    {
        var settings = await GetSettings();

        settings.Timezone = timezone;

        await _db.SaveChangesAsync();

        await RespondAsync($"Set timezone: {timezone}.");
    }

    private async Task<GuildSettings> GetSettings()
    {
        var settings = await _db.GuildSettings.FindAsync(Context.Guild.Id);

        if (settings == null)
        {
            settings = new GuildSettings { GuildId = Context.Guild.Id };
            _db.GuildSettings.Add(settings);
        }

        return settings;
    }
}
