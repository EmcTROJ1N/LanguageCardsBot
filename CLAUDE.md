# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

LanguageCardsBot is a .NET microservice system for a spaced-repetition Telegram bot. The current working directory is `src/ApiGateway` but the repository root is `/Users/OMON/Documents/LanguageCardsBot`.

## Build & Run

From the repository root:

```bash
dotnet restore LanguageCardsBot.sln
dotnet build LanguageCardsBot.sln
```

Run individual services:

```bash
dotnet run --project src/Cards.Presentation/Cards.Presentation.csproj
dotnet run --project src/Passport.Presentation/Passport.Presentation.csproj
dotnet run --project src/ApiGateway/ApiGateway.csproj
dotnet run --project src/EnglishCardsBot.Presentation/LanguageCardsBot.Presentation/LanguageCardsBot.Presentation.csproj
```

Full stack (per-service compose files, run from repo root):

```bash
docker network create language-cards-shared
docker network create passport
docker compose -f src/Cards.Presentation/docker-compose.yml up -d --build
docker compose -f src/Passport.Presentation/docker-compose.yml up -d --build
docker compose -f src/ApiGateway/docker-compose.yml up -d --build
```

There are no test projects. Minimum verification: `dotnet build LanguageCardsBot.sln`. Known build warning: nullable `CS8600` in `src/EnglishCardsBot.Presentation/LanguageCardsBot.Presentation/Workers/ReminderWorker.cs`.

## Architecture

Five services follow strict DDD layering (`Domain → Application → Infrastructure → Presentation`):

| Service | Framework | Port(s) | Database |
|---|---|---|---|
| `Cards.*` | .NET 8 | gRPC :8080, REST :8081 | MySQL 8.4 |
| `Passport.*` | .NET 10 | HTTP :8080 | Keycloak + MySQL :3307 |
| `ApiGateway` | .NET 8 | HTTP :5050 | — |
| `LanguageCardsBot.Presentation` (Telegram bot) | .NET 8 | — | — |
| `apps/chrome-extension` | Vanilla JS (MV3) | — | — |

**API Gateway (YARP)** routes:
- `/api/cards/**` → `cards-presentation:8081` (prefix stripped)
- `/api/passport/**` → `passport_presentation:8080` (prefix stripped)

**Telegram bot** talks to Cards via gRPC (not via the gateway).

**Chrome extension** calls Cards REST API directly on port 8081.

## gRPC Contracts

`.proto` files live exclusively in `src/LanguageCardsBot.Contracts.Cards/Protos/` — do not duplicate them in service projects. Consumers set `LanguageCardsBotGrpcServices` to `Server` or `Client` in their `.csproj`. Changing a proto requires updating both `Cards.Presentation/Services/` (server) and the Telegram bot project (client). Preserve existing field numbers; add new fields with new numbers only.

## EF Core & Migrations

Migrations are in `src/Cards.Infrastructure/Migrations/`. Use `CardsMysqlDbContextFactory` for design-time operations. Add a migration when changing persisted entity shape:

```bash
dotnet ef migrations add <MigrationName> --project src/Cards.Infrastructure --startup-project src/Cards.Presentation
```

## Configuration

- Copy `.env.example` to `.env`. Required: `BOT_TOKEN`, database connection strings.
- Cards service: `Database:ConnectionString` (or `DATABASE_CONNECTION_STRING`).
- Telegram bot gRPC: `Grpc:CardsServiceUrl`.
- Passport/Keycloak realm: `languagecards`; backend client: `languagecards-backend`.
- The gRPC endpoint (`Cards.Presentation:8080`) requires HTTP/2. Keep it separate from the REST port.

## Coding Conventions

- Target `net8.0` (Passport uses `net10.0`); nullable reference types and implicit usings enabled.
- File-scoped namespaces, constructor injection, async I/O throughout.
- XML documentation comments are required on all public types and members; add them to existing touched code that lacks them.
- Use UTC for any scheduling data crossing service boundaries.
- Telegram MarkdownV2: use existing `SendFormattedMessageAsync` helper for user-provided text. Callback data ≤ 64 bytes.
- Prefer `rg` for repository search.
