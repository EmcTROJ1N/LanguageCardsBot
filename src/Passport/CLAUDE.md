# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Service overview

Passport is the identity microservice for LanguageCardsBot. All user identity and authentication is delegated to Keycloak — there is no local user database, no EF Core, and no migrations in this service.

## Project structure

```
src/Passport/
  Passport.Domain/          # ValueObjects/, Abstractions/ — no EF Core
  Passport.Application/     # Abstractions/, Models/, Services/
  Passport.Infrastructure/  # Authentication/, Repositories/ — Keycloak.AuthServices.Sdk admin client
  Passport.Presentation/    # Controllers/, Authentication/, Models/, ServiceConfiguration.cs, Program.cs
```

## Run commands

### Local (without Docker)

```bash
cd src/Passport/Passport.Presentation
dotnet run
```

Scalar API reference is available at `http://localhost:5286/scalar` when `ASPNETCORE_ENVIRONMENT=Development`.

### Docker Compose

The `passport` Docker network must exist before first run:

```bash
docker network create passport
```

Then from `src/Passport/Passport.Presentation/`:

```bash
docker compose up --build
```

Services started:

| Container             | Host port | Default |
|-----------------------|-----------|---------|
| passport-presentation | 5286      | →8080   |
| keycloak              | 8082      | →8080   |
| passport-mysql        | 3307      | →3306   |

Keycloak admin UI: `http://localhost:8082` (admin / admin by default).

## Keycloak configuration

`appsettings.json` keys used by `Keycloak.AuthServices.Sdk`:

```json
"Keycloak": {
  "AuthServerUrl": "http://localhost:8082",
  "Realm": "languagecards",
  "Resource": "languagecards-backend",
  "Credentials": { "Secret": "<client-secret>" },
  "RequireHttpsMetadata": true
},
"KeycloakPublicClient": {
  "ClientId": "languagecardsbot-public",
  "TokenEndpoint": "http://localhost:8082/realms/languagecards/protocol/openid-connect/token"
}
```

In Docker Compose these are overridden via environment variables:

| Env var                    | Config key                           |
|----------------------------|--------------------------------------|
| `KEYCLOAK_AUTHORITY`       | `Keycloak__Authority`                |
| `KEYCLOAK_AUTH_SERVER_URL` | `Keycloak__AuthServerUrl`            |
| `KEYCLOAK_TOKEN_ENDPOINT`  | `KeycloakPublicClient__TokenEndpoint`|

## DI registration

Each layer owns its extension method; `Program.cs` calls them in order:

- `AddPassportInfrastructure` (`Passport.Infrastructure/DependencyInjection.cs`) — binds `KeycloakOptions`, registers `KeycloakAdminAuthHandler`, admin HTTP client (`AddKeycloakAdminHttpClient`), `IKeycloakTokenClient`.
- `AddPassportApplicationServices` (`Passport.Application/DependencyInjection.cs`) — registers `IAuthService` → `AuthService`.
- `AddPassportAuthentication` (`Presentation/ServiceConfiguration.cs`) — `AddKeycloakWebApiAuthentication` (JWT bearer).
- `AddPassportAuthorization` (`Presentation/ServiceConfiguration.cs`) — `AddKeycloakAuthorization` + policy `AdminAndUser` (realm role `User` + resource role `Admin`).

## DDD layer rules

- **Domain** — pure value objects and abstractions only. No EF Core, no infrastructure references.
- **Application** — interfaces (`Abstractions/`), command/result models (`Models/`), and service implementations that call Keycloak via abstractions. No HTTP clients directly.
- **Infrastructure** — Keycloak SDK clients, `KeycloakAdminAuthHandler`, and token repository. No domain logic.
- **Presentation** — ASP.NET Core controllers, `Program.cs`, `ServiceConfiguration.cs`. Wires all layers together.

Do not add EF Core to any layer — there is no relational schema owned by this service.
