# Language Cards Companion

Language Cards Companion is an Chrome Extension for adding cards to LanguageCardsBot from the browser. It is currently a lightweight Manifest V3 client without a build step, package manager, or bundled framework.

## Directory Layout

## Runtime Architecture

```text
Popup UI
  |
  | chrome.runtime.sendMessage(...)
  v
Background service worker
  |
  | CardsApiClient
  v
Cards.Presentation REST endpoint
```

The popup does not call the backend directly. It sends messages to the background service worker. The service worker loads settings from `chrome.storage.local`, validates required values, and calls the LanguageCardsBot HTTP API.

This keeps backend access in one place and makes future features easier to add, such as context menus, selected text capture, or keyboard commands.

## Chrome Platform Model

The extension uses Manifest V3.

Important manifest fields:

- `manifest_version: 3`: required for MV3 extensions.
- `action.default_popup`: points Chrome toolbar clicks to `popup/popup.html`.
- `background.service_worker`: registers `background/service-worker.js` as the extension event handler.
- `background.type: "module"`: allows ES module imports in the service worker.
- `options_page`: opens `options/options.html` for persistent settings.
- `permissions: ["storage"]`: allows `chrome.storage.local`.
- `host_permissions`: allows calls to local API hosts.

Current host permissions:

```json
[
  "http://localhost/*",
  "https://localhost/*",
  "http://127.0.0.1/*",
  "https://127.0.0.1/*"
]
```

Before production distribution, narrow these permissions to the real API host.

Official Chrome docs used for this structure:

- Manifest reference: https://developer.chrome.com/docs/extensions/reference/manifest
- Service workers in MV3: https://developer.chrome.com/docs/extensions/migrating/to-service-workers
- Extension messaging: https://developer.chrome.com/docs/extensions/develop/concepts/messaging
- Options pages and storage permission: https://developer.chrome.com/docs/extensions/develop/ui/options-page

## Backend Requirements

Run the cards backend with separated ports:

- gRPC: `http://localhost:8080`, HTTP/2
- REST: `http://localhost:8081`, HTTP/1

The extension must use the REST port.

Default extension setting:

```text
API base URL: http://localhost:8081
```

The Telegram bot should continue using the gRPC URL:

```text
Grpc:CardsServiceUrl=http://localhost:8080
```

## API Calls

The extension currently calls these REST endpoints on `Cards.Presentation`.

### Translate Term

```http
POST /api/cards/translation
Content-Type: application/json
```

Request:

```json
{
  "term": "example"
}
```

Expected response:

```json
{
  "result": {
    "translation": "пример",
    "transcription": "",
    "example": ""
  }
}
```

### Add Card

```http
POST /cards
Content-Type: application/json
```

Request:

```json
{
  "userId": 1,
  "term": "example",
  "translation": "пример",
  "transcription": "",
  "example": ""
}
```

Expected response:

```json
{
  "card": {
    "id": 1,
    "userId": 1,
    "term": "example",
    "translation": "пример",
    "transcription": "",
    "example": "",
    "level": 0,
    "nextReviewAt": null,
    "learned": false,
    "createdAt": "2026-05-21T00:00:00Z",
    "lastReviewAt": null,
    "totalReviews": 0,
    "correctReviews": 0
  }
}
```

## Extension Message Protocol

Popup code talks to the background service worker with `chrome.runtime.sendMessage`.

All successful responses have this shape:

```json
{
  "ok": true,
  "data": {}
}
```

All failed responses have this shape:

```json
{
  "ok": false,
  "error": "Human-readable error"
}
```

### `cards.translate`

Request:

```json
{
  "type": "cards.translate",
  "term": "example"
}
```

Behavior:

- validates that `term` is not empty;
- calls `POST /api/cards/v3/translation`;
- returns the API response.

### `cards.add`

Request:

```json
{
  "type": "cards.add",
  "card": {
    "term": "example",
    "translation": "пример",
    "transcription": "",
    "example": null
  }
}
```

Behavior:

- loads `userId` from extension settings;
- validates that `userId` is a positive integer;
- sends the card to `POST /cards`.

The popup does not send `userId`; the background worker injects it from settings.

### `settings.get`

Request:

```json
{
  "type": "settings.get"
}
```

Behavior:

- returns normalized extension settings from `chrome.storage.local`.

## Settings

Settings are stored in `chrome.storage.local`.

Current settings:

| Key | Default | Meaning |
| --- | --- | --- |
| `apiBaseUrl` | `http://localhost:8081` | Base URL of the REST API |
| `userId` | empty string | Internal LanguageCardsBot user id |

`apiBaseUrl` is normalized by trimming trailing slashes.

## Getting User ID

The Chrome extension needs the internal LanguageCardsBot user id.

In Telegram, send:

```text
/user_id
```

or:

```text
/id
```

The bot returns a numeric id. Put that number into the extension options page.

## Local Installation

1. Start the cards backend.

   ```bash
   dotnet run --project src/Cards.Presentation/Cards.Presentation.csproj
   ```

2. Open Chrome.
3. Go to:

   ```text
   chrome://extensions
   ```

4. Enable Developer mode.
5. Click Load unpacked.
6. Select:

   ```text
   apps/chrome-extension
   ```

7. Open the extension options page.
8. Set:

   ```text
   API base URL = http://localhost:8081
   User ID = value from /user_id in Telegram
   ```

## Local Usage

1. Click the Language Cards extension icon.
2. Enter a term.
3. Click Translate.
4. Review or edit:
   - Translation
   - Transcription
   - Example
5. Click Save card.

The card is created for the configured `User ID`.

## Development Workflow

Because there is no bundler, changes are immediate file edits.

After editing extension files:

1. Go to `chrome://extensions`.
2. Find Language Cards Companion.
3. Click Reload.
4. Reopen the popup.

When editing the background service worker:

1. Open `chrome://extensions`.
2. Find the extension.
3. Click service worker / Inspect views if Chrome shows it.
4. Check console logs and runtime errors there.

When editing the popup:

1. Right-click inside the popup.
2. Click Inspect.
3. Use the popup DevTools console.

## Verification Commands

Validate manifest JSON:

```bash
node -e "JSON.parse(require('fs').readFileSync('apps/chrome-extension/manifest.json', 'utf8'))"
```

Check JavaScript syntax:

```bash
node --check apps/chrome-extension/background/service-worker.js
node --check apps/chrome-extension/shared/api-client.js
node --check apps/chrome-extension/shared/config.js
node --check apps/chrome-extension/popup/popup.js
node --check apps/chrome-extension/options/options.js
```

Check backend build:

```bash
dotnet build src/Cards.Presentation/Cards.Presentation.csproj --no-restore
```

Check Docker Compose config:

```bash
docker compose -f src/Cards.Presentation/docker-compose.yml config
```

## Troubleshooting

### `Set a numeric LanguageCardsBot user id in extension options.`

The extension does not have a valid `userId`.

Fix:

1. Send `/user_id` to the Telegram bot.
2. Copy the returned number.
3. Open extension Options.
4. Paste it into User ID.
5. Save.

### `Failed to fetch`

The extension cannot reach the REST API.

Check:

- `Cards.Presentation` is running.
- REST endpoint is available on `http://localhost:8081`.
- Extension Options uses `http://localhost:8081`.
- `manifest.json` includes localhost host permissions.

### Translation returns 404

Check that `shared/api-client.js` calls:

```text
/api/cards/v3/translation
```

and that the backend controller still uses:

```csharp
[Route("api/cards/v3/translation")]
```

### Save card returns 400 or 500

Check:

- `term` is not empty;
- `translation` is present if backend validation requires it;
- `userId` exists in the cards database;
- database connection string is configured for `Cards.Presentation`.

### gRPC error appears while using the extension

The extension should not call the gRPC port.

Use:

```text
http://localhost:8081
```

Do not use:

```text
http://localhost:8080
```

`8080` is for gRPC over HTTP/2. Browser REST clients should use `8081`.

## Security Notes

The current extension trusts a manually entered internal user id. That is acceptable only for local development.

Before real users or public distribution:

- add proper authentication for browser clients;
- do not expose raw internal user ids as the only access control mechanism;
- restrict `host_permissions` to production API domains;
- use HTTPS for production API calls;
- avoid storing long-lived secrets in `chrome.storage.local` unless they are designed for that storage model;
- validate all card input server-side.

## API Gateway Notes

The extension currently points directly at `Cards.Presentation` REST port.

Long term, prefer:

```text
Chrome extension -> ApiGateway -> Cards.Presentation REST
```

That requires route prefixes to be normalized. Current backend routes are mixed:

- `CardsController`: `/cards`
- `TranslationController`: `/api/cards/v3/translation`
- `UsersController`: `/api/cards/v3/users`
- `StatsController`: `/api/cards/v3/stats`
- `CardsImportController`: `/import`

Before making `ApiGateway` the public browser boundary, decide on one external REST prefix and map all browser-facing routes through it.

## Future Work

Recommended next steps:

1. Add selected-text capture from the active tab.
2. Add a context menu action: Save selected text as card.
3. Add authentication for browser clients.
4. Move all browser-facing API calls through `ApiGateway`.
5. Add icons and production metadata.
6. Add a small test suite for API client behavior.
7. Add packaging scripts for release builds.

## File Ownership

`manifest.json` owns extension capabilities and permissions.

`background/service-worker.js` owns:

- message routing;
- settings lookup;
- API client creation;
- validation that should be shared across UI surfaces.

`shared/api-client.js` owns:

- REST endpoint paths;
- JSON request/response handling;
- API error extraction.

`shared/config.js` owns:

- default settings;
- settings normalization;
- saving settings to Chrome local storage.

`popup/` owns the main user flow.

`options/` owns extension configuration.
