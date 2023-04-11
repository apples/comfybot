using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

public class InteractionHandler
{
    private readonly IServiceProvider _services;
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _interactions;

    public InteractionHandler(
        IServiceProvider services,
        DiscordSocketClient client, 
        InteractionService interactions)
    {
        _services = services;
        _client = client;
        _interactions = interactions;
    }

    public async Task InitializeAsync()
    {
        await _interactions.AddModulesAsync(Assembly.GetExecutingAssembly(), _services);

        _client.InteractionCreated += HandleInteractionAsync;

        _interactions.Log += Log;

        _interactions.SlashCommandExecuted += SlashCommandExecuted;

        foreach (var module in _interactions.Modules)
        {
            Console.WriteLine($"{nameof(InteractionHandler)}: Module \"{module.Name}\" initialized.");
        }
    }

    private async Task HandleInteractionAsync(SocketInteraction arg)
    {
        try
        {
            var context = new SocketInteractionContext(_client, arg);
            await _interactions.ExecuteCommandAsync(context, _services);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private Task Log(LogMessage message)
    {
        Console.WriteLine($"[InteractionHandler/{message.Severity}] {message}");

		return Task.CompletedTask;
    }

    private async Task SlashCommandExecuted(SlashCommandInfo slashCommandInfo, IInteractionContext context, IResult result)
    {
        if (!result.IsSuccess)
        {
            switch (result.Error)
            {
                case InteractionCommandError.UnmetPrecondition:
                    await context.Interaction.RespondAsync($"Unmet Precondition: {result.ErrorReason}");
                    break;
                case InteractionCommandError.UnknownCommand:
                    await context.Interaction.RespondAsync("Unknown command");
                    break;
                case InteractionCommandError.BadArgs:
                    await context.Interaction.RespondAsync("Invalid number or arguments");
                    break;
                case InteractionCommandError.Exception:
                    await context.Interaction.RespondAsync($"Command exception: {result.ErrorReason}");
                    break;
                case InteractionCommandError.Unsuccessful:
                    await context.Interaction.RespondAsync("Command could not be executed");
                    break;
                default:
                    break;
            }
        }
    }

}
