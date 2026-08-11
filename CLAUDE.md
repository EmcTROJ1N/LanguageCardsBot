# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Services

| Service | Description | Port | CLAUDE.md |
|---|---|---|---|
| `src/ApiGateway/` | YARP reverse proxy | 5050 | [ApiGateway/CLAUDE.md](src/ApiGateway/CLAUDE.md) |
| `src/Cards/` | Cards gRPC+REST microservice (DDD) | — | [Cards/CLAUDE.md](src/Cards/CLAUDE.md) |
| `src/Contracts/` | Shared NuGet-packable gRPC contracts | — | [Contracts/CLAUDE.md](src/Contracts/CLAUDE.md) |
| `src/LanguageCardsBot/` | Telegram bot worker | — | [LanguageCardsBot/CLAUDE.md](src/LanguageCardsBot/CLAUDE.md) |
| `src/Passport/` | Identity/auth microservice (Keycloak) | — | [Passport/CLAUDE.md](src/Passport/CLAUDE.md) |

## Solution Build

```bash
dotnet build LanguageCardsBot.sln
```

Run this from the repo root before finishing any backend or contract change.

## Docker Network Bootstrap

Both external networks must exist before starting any compose stack:

```bash
docker network create language-cards-shared  # Cards + ApiGateway
docker network create passport               # Passport + Keycloak + ApiGateway
```

## Cross-Cutting Coding Standards

- **.NET 10** — nullable enabled, implicit usings, file-scoped namespaces, constructor injection, async I/O throughout.
- **DDD layer order** in microservices: Domain → Application → Infrastructure → Presentation.
- **XML docs** required on all public types and members; add to any touched member that lacks them.
- **UTC** for all cross-service timestamps.
- **Never commit** `.env` files, real tokens, or database dumps.
- **Do not revert** untracked or deleted files without being asked — they may belong to the user.
- **Search** with `rg` / `rg --files` rather than `find` or `grep`.
- **Local NuGet feed** lives at `.nuget/local/`; see `src/Contracts/CLAUDE.md` for pack/publish steps.
