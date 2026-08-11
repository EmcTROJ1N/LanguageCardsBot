# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Cards is a .NET 10 DDD microservice for spaced-repetition card storage. It exposes two transports simultaneously:

- **gRPC** on port `8080` (HTTP/2)
- **REST** on port `8081` (HTTP/1, Swagger at `/swagger`)

## Running Locally

### Without Docker

Set the connection string in `Cards.Presentation/appsettings.Development.json` (already pre-filled for a local MySQL on port 3306 with db/user/password `languagecards` / `languagecards` / `languagecards_password`), then:

```bash
dotnet run --project src/Cards/Cards.Presentation
```

### With Docker

The external Docker network must exist before starting:

```bash
docker network create language-cards-shared
```

Create `.env` next to `docker-compose.yml` with:

```
DATABASE_CONNECTION_STRING=Server=cards_mysql;Port=3306;Database=languagecards;User=...;Password=...;
MYSQL_DATABASE=languagecards
MYSQL_USER=...
MYSQL_PASSWORD=...
MYSQL_ROOT_PASSWORD=...
```

Then:

```bash
docker compose -f src/Cards/Cards.Presentation/docker-compose.yml up --build
```

The service joins the `language-cards-shared` network (defined as external in the compose file) so other services — e.g. ApiGateway — can reach it by container name `cards_presentation`.

## EF Core Migrations

Migrations live in `Cards.Infrastructure/Migrations/`. The design-time factory (`CardsMySqlDbContextFactory`) reads the connection string from `ConnectionStrings:CardsMysql` in `appsettings.json` / `appsettings.Development.json` inside `Cards.Infrastructure` (not `Cards.Presentation`).

Run migration commands from the repo root, targeting the Infrastructure project and using Presentation as the startup project:

```bash
# Add a new migration
dotnet ef migrations add <MigrationName> \
  --project src/Cards/Cards.Infrastructure \
  --startup-project src/Cards/Cards.Presentation

# Apply to the database
dotnet ef database update \
  --project src/Cards/Cards.Infrastructure \
  --startup-project src/Cards/Cards.Presentation
```

## gRPC Contracts

Proto-generated stubs come from the NuGet package `LanguageCardsBot.Contracts.Cards` (version pinned in `Cards.Presentation.csproj`). The project property `<LanguageCardsBotGrpcServices>Server</LanguageCardsBotGrpcServices>` tells the package to generate server-side stubs only. Do not add raw `.proto` files to this project.

Registered gRPC services: `CardGrpcService`, `CardsImportGrpcService`, `StatsGrpcService`, `UserGrpcService`, `GoogleTranslationService`.

## Layer Rules (DDD)

| Layer | Allowed dependencies |
|---|---|
| `Cards.Domain` | None (no project references) |
| `Cards.Application` | `Cards.Domain` only |
| `Cards.Infrastructure` | `Cards.Application`, `Cards.Domain` |
| `Cards.Presentation` | All layers (composition root) |

- **Domain** (`Entities/`, `ValueObjects/`, `Common/`): pure business logic, no EF Core attributes, no framework dependencies.
- **Application** (`Abstractions/`, `Cards/`, `Stats/`, `Translations/`, `Users/`, `Imports/`): use-case services, repository interfaces. No infrastructure types.
- **Infrastructure** (`Data/`, `Repositories/`, `Migrations/`): EF Core `CardsMysqlDbContext` with three `DbSet`s (`Users`, `Cards`, `Reviews`). Entity configurations in `Data/Configurations/`.
- **Presentation** (`Services/` for gRPC, `Controllers/` for REST, `Interceptors/`, `Mapping/`): registers everything via `ServiceConfiguration` extension methods; entry point is `Program.cs`.

## Configuration Reference

| Key | Source |
|---|---|
| `Database:ConnectionString` | `appsettings.json`, env `DATABASE_CONNECTION_STRING`, or env `DB_PATH` |
| `Translation:TargetLanguage` | `appsettings.json` or env `TRANSLATION_TARGET_LANGUAGE` (default `ru`) |
| `Translation:SourceLanguage` | `appsettings.json` or env `TRANSLATION_SOURCE_LANGUAGE` (default `auto`) |
