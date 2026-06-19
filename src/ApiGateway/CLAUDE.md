# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run

```bash
# From repo root
dotnet build LanguageCardsBot.sln

# Run locally
dotnet run --project src/ApiGateway/ApiGateway.csproj

# Run with Docker (requires external networks to exist)
docker compose -f src/ApiGateway/docker-compose.yml up -d
```

## What This Service Does

ApiGateway is a YARP reverse proxy with no business logic. It routes incoming HTTP requests to downstream microservices:

| Path prefix | Downstream service | Container address |
|---|---|---|
| `/api/cards/**` | Cards.Presentation (REST) | `http://cards-presentation:8081` |
| `/api/passport/**` | Passport.Presentation | `http://passport_presentation:8080` |

Routing config lives entirely in `appsettings.json` under `ReverseProxy`. Path prefixes are stripped before forwarding (`PathRemovePrefix` transform).

## Docker Networks

The gateway joins two external Docker networks — both must exist before `docker compose up`:

```bash
docker network create language-cards-shared  # connects to Cards service
docker network create passport               # connects to Passport service
```

The compose file (`docker-compose.yml`) exposes the gateway on port **5050**.

## Adding a New Route

1. Add a new route entry under `ReverseProxy.Routes` in `appsettings.json` with a `ClusterId` and `Match.Path`.
2. Add a corresponding cluster under `ReverseProxy.Clusters` with the downstream container address.
3. Add the new service's Docker network to `docker-compose.yml` under both `services.api-gateway.networks` and the top-level `networks` section (as an external network).
