using System;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using BigInteger = Org.BouncyCastle.Math.BigInteger;

using var db = new ComfyContext();

var config = new ConfigurationBuilder()
    .AddJsonFile($"appsettings.json", optional: true)
    .AddJsonFile($"appsettings.local.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

var client = new DiscordSocketClient(new DiscordSocketConfig
{
    GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent,
});

var commands = new CommandService(new CommandServiceConfig
{
    LogLevel = LogSeverity.Info,
    CaseSensitiveCommands = false,
});

var interactions = new InteractionService(client);

var services = new ServiceCollection();
services.AddSingleton<IServiceProvider>(sp => sp);
services.AddSingleton(client);
services.AddSingleton(commands);
services.AddSingleton(config);
services.AddSingleton(interactions);
services.AddDbContext<ComfyContext>();
services.AddSingleton<CommandHandler>();
services.AddSingleton<InteractionHandler>();
services.AddSingleton<EventScheduler>();
services.AddSingleton<SwizzleService>();

var serviceProvider = services.BuildServiceProvider();

await MainAsync();

async Task MainAsync()
{
    await serviceProvider.GetRequiredService<CommandHandler>().InitializeAsync();
    await serviceProvider.GetRequiredService<InteractionHandler>().InitializeAsync();
    await serviceProvider.GetRequiredService<EventScheduler>().InitializeAsync();
    await serviceProvider.GetRequiredService<SwizzleService>().InitializeAsync();
    
    client.Ready += async () =>
    {
        await interactions.RegisterCommandsGloballyAsync();

        Console.WriteLine($"Slash commands: {String.Join(", ", interactions.SlashCommands.Select(x => "/" + x.ToString()))}");

        Console.WriteLine($"Client is connected and ready!");
    };

    var token = config.GetRequiredSection("Settings")["DiscordBotToken"];
    if (string.IsNullOrWhiteSpace(token))
    {
        Console.WriteLine("ERROR: Settings.DiscordBotToken is null or empty.");
        return;
    }

    await client.LoginAsync(TokenType.Bot, token);
    await client.StartAsync();

    await Task.Delay(Timeout.Infinite);
}
