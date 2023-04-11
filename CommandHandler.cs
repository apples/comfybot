using System.Reflection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

public class CommandHandler
{
    private readonly IServiceProvider _services;
    private readonly DiscordSocketClient _client;
    private readonly CommandService _commands;

    public CommandHandler(
        IServiceProvider services,
        DiscordSocketClient client, 
        CommandService commands)
    {
        _services = services;
        _client = client;
        _commands = commands;
    }

    public async Task InitializeAsync()
    {
        await _commands.AddModulesAsync(Assembly.GetExecutingAssembly(), _services);
        
        _client.MessageReceived += HandleCommandAsync;
        
        _commands.CommandExecuted += async (optional, context, result) =>
        {
            if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
            {
                await context.Channel.SendMessageAsync($"error: {result}");
            }
        };

        foreach (var module in _commands.Modules)
        {
            Console.WriteLine($"{nameof(CommandHandler)}: Module \"{module.Name}\" initialized.");
        }
    }

    private async Task HandleCommandAsync(SocketMessage arg)
    {
        if (arg is not SocketUserMessage msg) 
            return;

        if (msg.Author.Id == _client.CurrentUser.Id || msg.Author.IsBot) 
            return;

        var context = new SocketCommandContext(_client, msg);
        
        var markPos = 0;
        if (msg.HasCharPrefix('!', ref markPos) || msg.HasCharPrefix('?', ref markPos))
        {
            var result = await _commands.ExecuteAsync(context, markPos, _services);
        }
    }
}
