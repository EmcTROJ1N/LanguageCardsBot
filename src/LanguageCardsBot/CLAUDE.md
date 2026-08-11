# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project overview

ASP.NET Core Worker Service (.NET 10) implementing a Telegram bot. It communicates with the Cards microservice exclusively via gRPC, using generated clients from the `LanguageCardsBot.Contracts.Cards` NuGet package.

Root namespace: `EnglishCardsBot.Presentation`  
Project file: `src/LanguageCardsBot/EnglishCardsBot.Presentation/LanguageCardsBot.Presentation/LanguageCardsBot.Presentation.csproj`

## Run command

```bash
dotnet run --project src/LanguageCardsBot/EnglishCardsBot.Presentation/LanguageCardsBot.Presentation/LanguageCardsBot.Presentation.csproj
```

A `.env` file in the project root is auto-loaded via `DotNetEnv` at startup.

## Required configuration

| Key | Source | Description |
|-----|--------|-------------|
| `BOT_TOKEN` | env var (preferred) | Telegram bot token |
| `Bot:Token` | appsettings / secrets | Fallback if `BOT_TOKEN` not set |
| `Grpc:CardsServiceUrl` | appsettings / env | gRPC address of the Cards service, e.g. `http://cards:5000` |
| `Bot:DailySummaryTime` | appsettings | Time for daily reminder (default `21:00:00`) |
| `Database:ConnectionString` | appsettings | SQLite path (default `Data Source=data/bot.db`) |

Startup throws `InvalidOperationException` if `BOT_TOKEN` or `Grpc:CardsServiceUrl` is missing.

## Architecture

- **`Worker`** — hosted service that starts `TelegramBotService` polling loop.
- **`ReminderWorker`** — hosted service that fires daily reminder messages.
- **`TelegramBotService`** — central dispatcher: receives Telegram updates, routes commands, handles callbacks and free-text input.
- **`Commands/`** — one folder per command, each handler implements `ICommandHandler`.

Do NOT add Telegram or bot logic to `Program.cs`. Route everything through `TelegramBotService` and command handlers.

## Supported bot commands

| Command | Handler class |
|---------|--------------|
| `/start` | `StartCommandHandler` |
| `/train` | `TrainCommandHandle` |
| `/stats` | `StatsCommandHandler` |
| `/list` | `ListCommandHandler` |
| `/cards` | (inline cards list, handled in `TelegramBotService`) |
| `/reminder_settings` | `ReminderSettingsCommandHandler` |
| `/clear` | `ClearCommandHandler` |
| `/export` | `ExportCommandHandler` |
| `/import` | `ImportCommandHandler` |
| `/userid` | `UserIdCommandHandler` |

Non-command text messages = card input flow. Documents = import flow. Both are handled in `TelegramBotService`.

## gRPC client setup

The `LanguageCardsBot.Contracts.Cards` package generates gRPC clients when the project property is set:

```xml
<LanguageCardsBotGrpcServices>Client</LanguageCardsBotGrpcServices>
```

Four clients are registered in DI (all pointing to `Grpc:CardsServiceUrl`):

- `UserService.UserServiceClient`
- `CardService.CardServiceClient`
- `StatsService.StatsServiceClient`
- `CardsImportService.CardsImportServiceClient`

All are in namespace `LanguageCardsBot.Contracts.Cards.V3`.

## Telegram-specific constraints

### MarkdownV2 escaping
Use `SendFormattedMessageAsync` (on `TelegramBotService`) when sending any user-provided or dynamic text. This method escapes the full string for MarkdownV2.

Do NOT use it when the message already contains intentional Markdown tokens (`*`, `||`, etc.) — those will be double-escaped. Build pre-formatted strings manually and call `botClient.SendMessage` with `ParseMode.MarkdownV2` directly in that case.

### Inline keyboard callback data
Telegram enforces a **64-byte hard limit** on callback data. Keep payloads short.

Current callback formats used by the cards list (examples — follow the same pattern):
- `cards:page:{p}`
- `cards:show:{cardId}:{p}`
- `cards:del:{cardId}:{p}`
- `cards:close`

### Timestamps
All timestamps exchanged with the Cards gRPC service must be **UTC**. Use `DateTime.UtcNow` / `DateTimeOffset.UtcNow`; never pass local time across service boundaries.
