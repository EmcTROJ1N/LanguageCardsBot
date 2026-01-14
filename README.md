# English Cards Bot - .NET 8 Implementation

Telegram bot for spaced repetition learning with DDD architecture.

## Architecture

- **Domain**: Entities, Value Objects
- **Application**: Use Cases, Interfaces, DTOs
- **Infrastructure**: Repositories, External Services
- **Presentation**: Worker Service, Telegram Bot Handlers

## Prerequisites

- .NET 8 SDK
- Docker and Docker Compose

## Configuration

1. Copy `.env.example` to `.env` in the Presentation project
2. Set `BOT_TOKEN` with your Telegram bot token

## Running with Docker

```bash
docker-compose up -d
```

## Running locally

```bash
cd src/EnglishCardsBot.Presentation/EnglishCardsBot.Presentation
dotnet run
```

## Features

- Add words with automatic or manual translation
- Spaced repetition algorithm
- Random reminders
- Daily statistics
- Export/Import cards
- Customizable reminder intervals
- Show/hide translations option

