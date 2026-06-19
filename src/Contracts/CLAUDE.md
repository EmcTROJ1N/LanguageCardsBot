# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this directory.

## Projects

### Contracts.Cards

NuGet-packable project (`PackageId: Contracts.Cards`, current version `3.0.1`). Does **not** emit build output — it ships only `.proto` files and a `.targets` file for code-gen.

- `Protos/` — all `.proto` files: `card_service`, `cards_import_service`, `common`, `stats_service`, `translation_service`, `user_service`.
- `buildTransitive/LanguageCardsBot.Contracts.Cards.targets` — injected into every consumer via NuGet's `buildTransitive` mechanism; wires all protos into the `<Protobuf>` MSBuild item group.
- `RootNamespace`: `LanguageCardsBot.Contracts`

### Contracts.Common

Contains shared enums (`Enum/`) and typed exceptions (`Exceptions/`) used across services. `PackageId: Contracts.Common`, version `1.0.0`. `RootNamespace`: `LanguageCardsBot.Common`.

## Packing and publishing to local source

The repo's `nuget.config` registers a local feed at `.nuget/local/` alongside nuget.org.

Pack `Contracts.Cards` into the local feed:
```
dotnet pack src/Contracts/Contracts.Cards/Contracts.Cards.csproj -o .nuget/local
```

Pack `Contracts.Common` into the local feed:
```
dotnet pack src/Contracts/Contracts.Common/Contracts.Common.csproj -o .nuget/local
```

After packing, consumer projects pick up the new version automatically on the next restore (no extra `nuget add source` step needed).

## How consumers wire up gRPC code-gen

The `.targets` file is delivered transitively by the NuGet package. It sets the default value of `LanguageCardsBotGrpcServices` to `Both` when the property is not already set. Consumers override it in their `.csproj`:

| Project | Property value | Effect |
|---|---|---|
| `Cards.Presentation` (server) | `Server` | Generates server-side stubs only |
| Telegram bot (client) | `Client` | Generates client-side stubs only |

Example consumer `.csproj` snippet:
```xml
<PropertyGroup>
  <LanguageCardsBotGrpcServices>Client</LanguageCardsBotGrpcServices>
</PropertyGroup>
```

No `<Protobuf>` items need to be declared manually — the `.targets` file handles them.

## Proto file rules

- **Never copy `.proto` files into service projects.** All protos live exclusively under `src/Contracts/Contracts.Cards/Protos/`.
- **Preserve all existing field numbers.** Changing or reusing a field number is a breaking wire-format change.
- **Add new fields with new, previously-unused numbers only.**
- **Bump the package version** in `Contracts.Cards.csproj` whenever protos change, then re-pack to `.nuget/local`.

## Versioning

- Follow semantic versioning: breaking proto changes → major bump; new fields/services → minor bump; non-functional changes → patch.
- Update `<Version>` in the relevant `.csproj` before packing. Update consumer `PackageReference` versions after confirming the new package is in `.nuget/local`.
