# AGENTS.md

## Project Overview

LanguageCardsBot is a .NET 8 microservice-style Telegram bot for spaced-repetition language cards. The current product surface is a Telegram bot used to add, review, import, export, and list cards for memorizing words or phrases.

The repository is organized around a cards backend exposed through gRPC and a separate Telegram worker that calls that backend. All microservices are currently implemented using a Domain-Driven Design strategy, and future work should preserve that approach.

## Repository Layout

- `LanguageCardsBot.sln` - root solution.
- `src/LanguageCardsBot.Contracts.Cards` - NuGet-packable gRPC contract project. Owns `.proto` files under `Protos/` and `buildTransitive/LanguageCardsBot.Contracts.Cards.targets`.
- `src/Cards.Domain` - card, review, and user domain entities/value objects.
- `src/Cards.Infrastructure` - EF Core repositories, MySQL `DbContext`, entity configuration, and migrations.
- `src/Cards.Presentation` - ASP.NET Core gRPC server implementing the cards contracts.
- `src/EnglishCardsBot.Presentation/LanguageCardsBot.Presentation` - Telegram bot worker service and command handlers. Despite the parent folder name, the active project is `LanguageCardsBot.Presentation.csproj`.
- `docker-compose.yml` - root compose file for MySQL, the gRPC service, and the Telegram bot.

## Architecture Rules

- Follow the existing DDD strategy for every microservice: keep domain models and business rules in domain projects, infrastructure concerns in infrastructure projects, and transport/UI concerns in presentation projects.
- Keep the contract boundary explicit: Telegram bot code should talk to cards data through generated gRPC clients from `LanguageCardsBot.Contracts.Cards`, not by referencing infrastructure repositories directly.
- Keep cards business state in the cards service side: domain entities and repository behavior belong under `Cards.Domain` and `Cards.Infrastructure`.
- Keep Telegram interaction logic in the bot worker project: command parsing, Telegram messages, callbacks, and background reminder workers belong under `LanguageCardsBot.Presentation`.
- Do not duplicate `.proto` contracts in service projects. Edit proto files in `src/LanguageCardsBot.Contracts.Cards/Protos/`.
- When changing proto contracts, update both server implementations in `Cards.Presentation/Services` and client usage in the Telegram bot project.
- Preserve backward compatibility in existing proto field numbers. Add new fields with new numbers; do not renumber or reuse removed fields.

## Build And Run

Use the solution from the repository root:

```bash
dotnet restore LanguageCardsBot.sln
dotnet build LanguageCardsBot.sln
```

Run the cards gRPC service locally:

```bash
dotnet run --project src/Cards.Presentation/Cards.Presentation.csproj
```

Run the Telegram worker locally:

```bash
dotnet run --project src/EnglishCardsBot.Presentation/LanguageCardsBot.Presentation/LanguageCardsBot.Presentation.csproj
```

Run the full stack with Docker Compose:

```bash
docker compose up --build
```

There are currently no test projects in the solution. For code changes, at minimum run `dotnet build LanguageCardsBot.sln`. Add focused tests when introducing test infrastructure or changing non-trivial domain/repository behavior.

## Configuration

- Use `.env.example` as the root Docker Compose template. Do not commit real secrets.
- Required bot setting: `BOT_TOKEN`.
- Required bot gRPC setting: `Grpc:CardsServiceUrl` or `Grpc__CardsServiceUrl`.
- Required cards service database setting: `Database:ConnectionString`, `Database__ConnectionString`, `DATABASE_CONNECTION_STRING`, or `DB_PATH`.
- The cards backend is configured for MySQL via EF Core. The root compose file exposes MySQL on `3306` and the cards gRPC service on `8080`.
- `Cards.Presentation` requires HTTP/2 for gRPC. Keep Kestrel protocol settings compatible with gRPC.

## NuGet And Contracts

- `LanguageCardsBot.Contracts.Cards` is packable and currently versioned in its `.csproj`.
- The contracts package includes proto files and a build-transitive target that generates gRPC code for consumers.
- Consumers set `LanguageCardsBotGrpcServices` to choose generated service type:
  - `Server` in `Cards.Presentation`.
  - `Client` in the Telegram bot worker project.
- If a contract package version changes, keep package references in service projects aligned.

## Coding Guidelines

- Target framework is `net8.0`; nullable reference types and implicit usings are enabled.
- Follow existing C# style: file-scoped namespaces, constructor injection where already used, async APIs for I/O and gRPC calls.
- Keep comments sparse and useful. Do not add comments that simply restate code.
- Use UTC for reminder and review scheduling data that crosses service boundaries.
- Keep Telegram MarkdownV2 escaping in mind. Use existing helpers such as `SendFormattedMessageAsync` when sending user-provided text.
- Keep callback payloads short; Telegram callback data is limited to 64 bytes.
- Avoid broad refactors unless they are required for the requested change.

## Database

- EF Core migrations live in `src/Cards.Infrastructure/Migrations`.
- `CardsMysqlDbContextFactory` is used for design-time EF operations.
- Add migrations in `Cards.Infrastructure` when changing persisted entity shape.
- Do not rely on local SQLite files such as `identifier.sqlite` for service data unless the code explicitly does so.

## Telegram Bot Behavior

The bot currently supports command handlers for:

- `/start`
- `/train`
- `/stats`
- `/list` and `/cards`
- `/reminder_settings`
- `/clear`
- `/export`
- `/import`

Text messages that are not commands are handled as card input. Documents are handled by the import flow. Keep these flows routed through `TelegramBotService` and command handlers rather than adding unrelated logic to `Program.cs`.

## Verification Notes

- Prefer `rg`/`rg --files` for repository search.
- Before finishing backend or contract work, run `dotnet build LanguageCardsBot.sln` when possible.
- If Docker behavior changes, verify with `docker compose config` or `docker compose up --build` when the environment permits.
- Current known build warning observed during inspection: nullable warning `CS8600` in `src/EnglishCardsBot.Presentation/LanguageCardsBot.Presentation/Workers/ReminderWorker.cs`.

## Git Hygiene

- Do not commit `.env`, real Telegram tokens, database dumps, or local IDE/system files.
- Existing untracked or deleted files may belong to the user. Do not revert unrelated working tree changes unless explicitly asked.
