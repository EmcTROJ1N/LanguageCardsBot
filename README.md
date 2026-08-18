# LanguageCardsBot

Telegram-бот для изучения слов методом интервальных повторений. Микросервисная архитектура на .NET 10.

## Сервисы

| Сервис | Описание | Порт |
|---|---|---|
| `ApiGateway` | YARP reverse proxy | 5050 |
| `Cards` | gRPC + REST, хранение карточек | 8080 / 8081 |
| `Passport` | Аутентификация (Keycloak) | 5286 |
| `LanguageCardsBot` | Telegram bot worker | — |

## Запуск

### 1. Создать внешние сети (один раз)

```bash
docker network create language-cards-shared
docker network create passport
docker network create infra
```

### 2. Настроить переменные окружения для бота

```bash
cp src/LanguageCardsBot/LanguageCardsBot.Presentation/LanguageCardsBot.Presentation/.env.example \
   src/LanguageCardsBot/LanguageCardsBot.Presentation/LanguageCardsBot.Presentation/.env
```

Открыть `.env` и заполнить `BOT_TOKEN`.

### 3. Запустить весь стек

```bash
docker compose up --build
```

## Требования

- Docker + Docker Compose v2.20+
- .NET 10 SDK (только для локальной разработки)
