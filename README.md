# ComfyBot

## Setup

First, make an `appsettings.local.json` file.

Get your Discord bot token, and put it into `appsettings.local.json`.

DO NOT EVER NEVER EVER put the Discord token into `appsettings.json` (this is the wrong file!).

Launch the bot with `dotnet run`.

## Database Migrations

```
dotnet ef migrations add Whatever
dotnet ef database update
```
