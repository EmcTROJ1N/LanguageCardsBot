# Language Cards Companion

Language Cards Companion is an Chrome Extension for adding cards to LanguageCardsBot from the browser. It is currently a lightweight Manifest V3 client without a build step, package manager, or bundled framework.

## Directory Layout

```text
apps/chrome-extension/
  manifest.json
  background/service-worker.js       Message router, API client owner, dynamic content-script registrations
  popup/                             Toolbar popup UI
  options/                           Options page (settings, custom providers, auth status)
  content/
    translation-tooltip.js           Shared translate/save tooltip (window.__lcTranslationTooltip)
    selection-translator.js          Global text-selection translator (all URLs)
    subtitle-clicker.js              Per-host subtitle word clicker
    token-sync.js                    Web-app-origin script that syncs auth tokens into chrome.storage
    tooltip.css                      Styles for tooltip and selection icon
  shared/                            API client and settings helpers (background only)
```

## Runtime Architecture

```text
Popup UI                     Content scripts (selection-translator, subtitle-clicker)
       \                    /
        \                  /
         chrome.runtime.sendMessage(...)
                  |
                  v
         Background service worker
                  |
                  | CardsApiClient
                  v
         Cards.Presentation REST endpoint
```

The popup and every content script send messages to the background service worker. The service worker loads settings from `chrome.storage.local`, validates required values, and calls the LanguageCardsBot HTTP API. Content scripts never call the backend directly.

There are two content-script entry points:

- `content/selection-translator.js` is declared in `manifest.json` under `content_scripts` with `matches: ["<all_urls>"]` and runs on every page. It shows a small icon next to a text selection and opens the shared tooltip on click.
- `content/subtitle-clicker.js` is registered per host by the service worker (`subtitles.enable` / `subtitles.disable`) and only runs on sites the user opts into from the popup.

Both entries reuse `content/translation-tooltip.js`, which exposes `window.__lcTranslationTooltip.show({ term, anchorRect, context, onClose })` and encapsulates the translate/save flow.

A third content script, `content/token-sync.js`, is registered dynamically by the service worker for the current `apiBaseUrl` origin. When the web SPA is loaded there, it reads `auth_token` / `refresh_token` from `localStorage` and forwards them to the background via `auth.sync`. The background stores them in `chrome.storage.local`, and `shared/api-client.js` attaches them as `Authorization: Bearer` on every request (with a one-shot refresh on `401`).

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

The extension talks to the API through the YARP `ApiGateway`, the same URL the web SPA is served from. In local development that is:

```text
API base URL: http://localhost:5050
```

Under this URL both the SPA and the API are reachable:

- `http://localhost:5050/` — web SPA (used for login and for token capture)
- `http://localhost:5050/api/cards/**` — Cards service
- `http://localhost:5050/api/passport/**` — Passport (auth) service

The Telegram bot continues to use the internal gRPC URL:

```text
Grpc:CardsServiceUrl=http://localhost:8080
```

## API Calls

The extension calls these endpoints through the gateway, always with an `Authorization: Bearer <accessToken>` header.

### Translate Term

```http
POST /api/cards/v3/translation
Authorization: Bearer <accessToken>
Content-Type: application/json
```

Request:

```json
{ "term": "example" }
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
POST /api/cards/cards
Authorization: Bearer <accessToken>
Content-Type: application/json
```

Request (the user is derived from the token — no `userId` field):

```json
{
  "term": "example",
  "translation": "пример",
  "transcription": "",
  "example": null
}
```

### Refresh Token

Called automatically on `401` responses:

```http
POST /api/passport/v1/auth/refresh
Content-Type: application/json

"<refreshToken>"
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

- sends the card to `POST /api/cards/cards` with the current auth token;
- the backend derives the user from the token — no `userId` is sent.

### `settings.get`

Request:

```json
{
  "type": "settings.get"
}
```

Behavior:

- returns normalized extension settings from `chrome.storage.local`.

### `auth.sync`

Sent by `content/token-sync.js` from the web app origin.

Request:

```json
{
  "type": "auth.sync",
  "authToken": "<access token from web localStorage>",
  "refreshToken": "<refresh token from web localStorage>"
}
```

Behavior:

- writes both tokens to `chrome.storage.local`;
- if both tokens are empty, clears them (logout).

### `auth.status`

Request:

```json
{ "type": "auth.status" }
```

Response `data`:

```json
{ "signedIn": true }
```

## Settings and Storage

Settings and tokens are all stored in `chrome.storage.local`.

| Key | Default | Meaning |
| --- | --- | --- |
| `apiBaseUrl` | `http://localhost:5050` | URL of the API gateway (also the origin of the web SPA) |
| `authToken` | empty | Access token synced from web `localStorage.auth_token` |
| `refreshToken` | empty | Refresh token synced from web `localStorage.refresh_token` |
| `customProviders` | `[]` | Extra subtitle providers configured in Options |

`apiBaseUrl` is normalized by trimming trailing slashes. Tokens are written and cleared by `content/token-sync.js` running on the API base URL origin, and refreshed by `shared/api-client.js` on 401.

## Signing In

The extension has no login UI of its own — it reuses the web app's session.

1. Open `apiBaseUrl` in a browser tab (`http://localhost:5050` locally).
2. Log in through the web app.
3. The extension's `content/token-sync.js` picks up the `auth_token` / `refresh_token` from `localStorage` and stores them in `chrome.storage.local`.
4. Every extension API call attaches `Authorization: Bearer <accessToken>`. On `401`, the extension refreshes once via `/api/passport/v1/auth/refresh`.

Logging out from the web app clears both `localStorage` entries; `token-sync.js` forwards the empty values, and the extension clears its own copies on the next `auth.sync` message.

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
8. Confirm `API base URL = http://localhost:5050` (the API gateway / web SPA origin).
9. Open `http://localhost:5050` in a tab, log in through the web app. The extension picks up the token automatically.

## Local Usage

### Popup flow

1. Click the Language Cards extension icon.
2. Enter a term.
3. Click Translate.
4. Review or edit:
   - Translation
   - Transcription
   - Example
5. Click Save card.

The card is created for the configured `User ID`.

### Selection translator (all pages)

1. On any page, select a short piece of text with the mouse (up to 100 characters).
2. A small `A↔` icon appears near the right edge of the selection.
3. Click the icon.
4. In the tooltip that appears, click Перевести to load the translation.
5. Click Сохранить to create a card. `example` is auto-filled with the sentence around the selection (up to 500 characters).

The icon is skipped inside `<input>`, `<textarea>` and `contenteditable` regions, and disappears when you scroll, resize, or click outside.

### Subtitle clicker (per host)

1. Open a video page (YouTube, Netflix, kino.pub, or a custom provider configured in Options).
2. Open the extension popup and turn the Subtitles toggle On for the current host.
3. Reload the video page.
4. Click a word inside the subtitles to open the translate/save tooltip.

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
node --check apps/chrome-extension/content/translation-tooltip.js
node --check apps/chrome-extension/content/selection-translator.js
node --check apps/chrome-extension/content/subtitle-clicker.js
node --check apps/chrome-extension/content/token-sync.js
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

### `Sign in via web app to use the extension.`

The extension has no valid token (either never synced, or the refresh failed).

Fix:

1. Open `apiBaseUrl` (default `http://localhost:5050`) in a browser tab.
2. Log in through the web app.
3. Reload the extension popup / retry the action. The Options page should show "Signed in via web app…".

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
- the user in the token has been provisioned on the Cards side (usually via first web login);
- database connection string is configured for `Cards.Presentation`.

### gRPC error appears while using the extension

The extension only talks to the API gateway REST port. Do not point `apiBaseUrl` at `http://localhost:8080` — that is gRPC over HTTP/2.

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

`manifest.json` owns extension capabilities, permissions, and the global content-script registration for the selection translator.

`background/service-worker.js` owns:

- message routing;
- settings lookup;
- API client creation;
- validation that should be shared across UI surfaces;
- per-host registration of the subtitle clicker (and its shared tooltip dependency).

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

`content/translation-tooltip.js` owns the translate/save tooltip DOM and lifecycle, exposed as `window.__lcTranslationTooltip`. Both subtitle-clicker and selection-translator delegate to it.

`content/selection-translator.js` owns the global selection-icon flow: watches `mouseup`, extracts the surrounding sentence, and invokes the shared tooltip.

`content/subtitle-clicker.js` owns per-host subtitle overlays and delegates word clicks to the shared tooltip while pausing/resuming the underlying `<video>`.

`content/token-sync.js` owns web-app-origin token capture: reads `auth_token` / `refresh_token` from `localStorage` and forwards them to the background via `auth.sync`.
