# Subtitle Clicker Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Allow the user to click any subtitle word on kino.pub (and other configurable sites), translate it, and save it as a vocabulary card — activated per-domain via the extension popup.

**Architecture:** A content script (`subtitle-clicker.js`) is injected on-demand via `chrome.scripting.registerContentScripts()` when the user enables the feature for a given domain in the popup. On load, the script selects the right subtitle provider (TextTrack → custom → built-in) and wraps subtitle words in clickable spans. Clicking a word pauses the video and shows a tooltip for manual translate → save flow.

**Tech Stack:** Vanilla JS (no build step), Chrome Extensions Manifest V3, `chrome.scripting` API, `chrome.storage.local`.

---

### Task 1: Update manifest.json and delete subtitle-logger.js

**Files:**
- Modify: `apps/chrome-extension/manifest.json`
- Delete: `apps/chrome-extension/content/subtitle-logger.js`

- [ ] **Step 1: Write new manifest.json**

Replace the full contents of `apps/chrome-extension/manifest.json`:

```json
{
  "manifest_version": 3,
  "name": "Language Cards Companion",
  "description": "Chrome companion for adding and translating LanguageCardsBot cards through the HTTP API.",
  "version": "0.1.0",
  "action": {
    "default_title": "Language Cards",
    "default_popup": "popup/popup.html"
  },
  "background": {
    "service_worker": "background/service-worker.js",
    "type": "module"
  },
  "options_page": "options/options.html",
  "permissions": [
    "storage",
    "scripting",
    "tabs"
  ],
  "host_permissions": [
    "<all_urls>"
  ]
}
```

Note: `<all_urls>` in `host_permissions` is required so `chrome.scripting.registerContentScripts()` can inject into any domain the user enables. It subsumes the previous localhost entries.

- [ ] **Step 2: Delete the diagnostic content script**

```bash
rm apps/chrome-extension/content/subtitle-logger.js
```

- [ ] **Step 3: Validate manifest JSON**

```bash
node -e "JSON.parse(require('fs').readFileSync('apps/chrome-extension/manifest.json','utf8')); console.log('OK')"
```

Expected: `OK`

- [ ] **Step 4: Commit**

```bash
git add apps/chrome-extension/manifest.json
git rm apps/chrome-extension/content/subtitle-logger.js
git commit -m "feat(extension): replace static content_scripts with dynamic scripting API"
```

---

### Task 2: Add custom provider helpers to shared/config.js

**Files:**
- Modify: `apps/chrome-extension/shared/config.js`

The file currently exports `getSettings` and `saveSettings`. We add two helpers for storing user-defined subtitle providers in `chrome.storage.local` under the key `customProviders`.

- [ ] **Step 1: Write updated config.js**

Replace the full contents of `apps/chrome-extension/shared/config.js`:

```js
const DEFAULT_SETTINGS = {
  apiBaseUrl: "http://localhost:8081",
  userId: ""
};

export async function getSettings() {
  const stored = await chrome.storage.local.get(DEFAULT_SETTINGS);
  return {
    apiBaseUrl: normalizeBaseUrl(stored.apiBaseUrl),
    userId: String(stored.userId ?? "").trim()
  };
}

export async function saveSettings(settings) {
  const nextSettings = {
    apiBaseUrl: normalizeBaseUrl(settings.apiBaseUrl || DEFAULT_SETTINGS.apiBaseUrl),
    userId: String(settings.userId ?? "").trim()
  };

  await chrome.storage.local.set(nextSettings);
  return nextSettings;
}

export async function getCustomProviders() {
  const result = await chrome.storage.local.get({ customProviders: [] });
  return result.customProviders;
}

export async function saveCustomProviders(providers) {
  await chrome.storage.local.set({ customProviders: providers });
}

function normalizeBaseUrl(value) {
  return String(value ?? DEFAULT_SETTINGS.apiBaseUrl).trim().replace(/\/+$/, "");
}
```

- [ ] **Step 2: Verify syntax**

```bash
node --check apps/chrome-extension/shared/config.js && echo "OK"
```

Expected: `OK`

- [ ] **Step 3: Commit**

```bash
git add apps/chrome-extension/shared/config.js
git commit -m "feat(extension): add getCustomProviders/saveCustomProviders to config"
```

---

### Task 3: Add subtitle scripting handlers to service-worker.js

**Files:**
- Modify: `apps/chrome-extension/background/service-worker.js`

Add three new message types: `subtitles.getStatus`, `subtitles.enable`, `subtitles.disable`. Each is handled inside the existing `switch` statement before the `default` case.

`subtitles.enable` registers a persistent content script for the given hostname (survives page reloads and browser restarts), then immediately injects it into the current tab so the user does not have to reload.

- [ ] **Step 1: Write updated service-worker.js**

Replace the full contents of `apps/chrome-extension/background/service-worker.js`:

```js
import { CardsApiClient } from "../shared/api-client.js";
import { getSettings } from "../shared/config.js";

chrome.runtime.onMessage.addListener((message, sender, sendResponse) => {
  handleMessage(message)
    .then((data) => sendResponse({ ok: true, data }))
    .catch((error) => sendResponse({ ok: false, error: error.message }));

  return true;
});

async function handleMessage(message) {
  const settings = await getSettings();
  const client = new CardsApiClient(settings.apiBaseUrl);

  switch (message?.type) {
    case "cards.translate":
      return client.translate(requireText(message.term, "term"));

    case "cards.add":
      return client.addCard({
        ...message.card,
        userId: requireUserId(settings.userId)
      });

    case "settings.get":
      return settings;

    case "subtitles.getStatus": {
      const id = `subtitle-clicker-${message.hostname}`;
      const scripts = await chrome.scripting.getRegisteredContentScripts({ ids: [id] });
      return { enabled: scripts.length > 0 };
    }

    case "subtitles.enable": {
      const id = `subtitle-clicker-${message.hostname}`;
      const pattern = `*://${message.hostname}/*`;
      const existing = await chrome.scripting.getRegisteredContentScripts({ ids: [id] });
      if (existing.length === 0) {
        await chrome.scripting.registerContentScripts([{
          id,
          matches: [pattern],
          js: ["content/subtitle-clicker.js"],
          css: ["content/tooltip.css"],
          runAt: "document_idle"
        }]);
      }
      const [tab] = await chrome.tabs.query({ active: true, currentWindow: true });
      if (tab?.id) {
        await chrome.scripting.insertCSS({
          target: { tabId: tab.id },
          files: ["content/tooltip.css"]
        }).catch(() => {});
        await chrome.scripting.executeScript({
          target: { tabId: tab.id },
          files: ["content/subtitle-clicker.js"]
        }).catch(() => {});
      }
      return {};
    }

    case "subtitles.disable": {
      const id = `subtitle-clicker-${message.hostname}`;
      const existing = await chrome.scripting.getRegisteredContentScripts({ ids: [id] });
      if (existing.length > 0) {
        await chrome.scripting.unregisterContentScripts({ ids: [id] });
      }
      return {};
    }

    default:
      throw new Error("Unsupported extension message.");
  }
}

function requireText(value, name) {
  const text = String(value ?? "").trim();
  if (!text) throw new Error(`${name} is required.`);
  return text;
}

function requireUserId(value) {
  const userId = Number.parseInt(value, 10);
  if (!Number.isInteger(userId) || userId <= 0) {
    throw new Error("Set a numeric LanguageCardsBot user id in extension options.");
  }
  return userId;
}
```

- [ ] **Step 2: Verify syntax**

```bash
node --check apps/chrome-extension/background/service-worker.js && echo "OK"
```

Expected: `OK`

- [ ] **Step 3: Commit**

```bash
git add apps/chrome-extension/background/service-worker.js
git commit -m "feat(extension): add subtitles enable/disable/getStatus message handlers"
```

---

### Task 4: Add subtitle toggle to popup

**Files:**
- Modify: `apps/chrome-extension/popup/popup.html`
- Modify: `apps/chrome-extension/popup/popup.js`
- Modify: `apps/chrome-extension/popup/popup.css`

Add a toggle row between the header and the Term label. The toggle reads the current hostname, queries `subtitles.getStatus` from the service worker, and sends `subtitles.enable` / `subtitles.disable` on click.

- [ ] **Step 1: Update popup.html**

Replace the full contents of `apps/chrome-extension/popup/popup.html`:

```html
<!doctype html>
<html lang="en">
  <head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <title>Language Cards</title>
    <link rel="stylesheet" href="popup.css">
  </head>
  <body>
    <main class="popup">
      <header class="header">
        <h1>Language Cards</h1>
        <a href="../options/options.html" target="_blank" rel="noreferrer">Options</a>
      </header>

      <div class="subtitle-row">
        <span class="subtitle-label">Subtitle clicker</span>
        <button id="subtitleToggle" type="button" class="toggle-btn" aria-pressed="false">Off</button>
      </div>

      <label>
        Term
        <textarea id="term" rows="3" autocomplete="off" spellcheck="true"></textarea>
      </label>

      <div class="actions">
        <button id="translate" type="button">Translate</button>
        <button id="save" type="button">Save card</button>
      </div>

      <section class="fields" aria-label="Card details">
        <label>
          Translation
          <input id="translation" type="text" autocomplete="off">
        </label>
        <label>
          Transcription
          <input id="transcription" type="text" autocomplete="off">
        </label>
        <label>
          Example
          <textarea id="example" rows="3" autocomplete="off" spellcheck="true"></textarea>
        </label>
      </section>

      <p id="status" class="status" role="status"></p>
    </main>

    <script type="module" src="popup.js"></script>
  </body>
</html>
```

- [ ] **Step 2: Update popup.js**

Replace the full contents of `apps/chrome-extension/popup/popup.js`:

```js
const elements = {
  term: document.querySelector("#term"),
  translation: document.querySelector("#translation"),
  transcription: document.querySelector("#transcription"),
  example: document.querySelector("#example"),
  status: document.querySelector("#status"),
  translate: document.querySelector("#translate"),
  save: document.querySelector("#save"),
  subtitleToggle: document.querySelector("#subtitleToggle")
};

elements.translate.addEventListener("click", translateTerm);
elements.save.addEventListener("click", saveCard);
elements.subtitleToggle.addEventListener("click", toggleSubtitleClicker);

let currentHostname = null;
initSubtitleToggle();

async function initSubtitleToggle() {
  try {
    const [tab] = await new Promise(resolve =>
      chrome.tabs.query({ active: true, currentWindow: true }, resolve)
    );
    if (!tab?.url || !tab.url.startsWith("http")) {
      elements.subtitleToggle.disabled = true;
      return;
    }
    currentHostname = new URL(tab.url).hostname;
    const response = await sendMessage({ type: "subtitles.getStatus", hostname: currentHostname });
    setToggleState(response.enabled);
  } catch {
    elements.subtitleToggle.disabled = true;
  }
}

async function toggleSubtitleClicker() {
  if (!currentHostname) return;
  const isOn = elements.subtitleToggle.getAttribute("aria-pressed") === "true";
  await runAction(async () => {
    if (isOn) {
      await sendMessage({ type: "subtitles.disable", hostname: currentHostname });
      setToggleState(false);
    } else {
      await sendMessage({ type: "subtitles.enable", hostname: currentHostname });
      setToggleState(true);
    }
  });
}

function setToggleState(enabled) {
  elements.subtitleToggle.setAttribute("aria-pressed", String(enabled));
  elements.subtitleToggle.textContent = enabled ? "On" : "Off";
  elements.subtitleToggle.classList.toggle("is-on", enabled);
}

async function translateTerm() {
  await runAction(async () => {
    const response = await sendMessage({
      type: "cards.translate",
      term: elements.term.value
    });

    const result = response.result;
    elements.translation.value = result.translation ?? "";
    elements.transcription.value = result.transcription ?? "";
    elements.example.value = result.example ?? "";
    setStatus("Translation loaded.");
  });
}

async function saveCard() {
  await runAction(async () => {
    await sendMessage({
      type: "cards.add",
      card: {
        term: elements.term.value.trim(),
        translation: elements.translation.value.trim(),
        transcription: elements.transcription.value.trim(),
        example: elements.example.value.trim() || null
      }
    });

    setStatus("Card saved.");
  });
}

async function runAction(action) {
  setBusy(true);
  setStatus("");

  try {
    await action();
  } catch (error) {
    setStatus(error.message, true);
  } finally {
    setBusy(false);
  }
}

function sendMessage(message) {
  return new Promise((resolve, reject) => {
    chrome.runtime.sendMessage(message, (response) => {
      if (chrome.runtime.lastError) {
        reject(new Error(chrome.runtime.lastError.message));
        return;
      }

      if (!response?.ok) {
        reject(new Error(response?.error ?? "Extension request failed."));
        return;
      }

      resolve(response.data);
    });
  });
}

function setBusy(isBusy) {
  elements.translate.disabled = isBusy;
  elements.save.disabled = isBusy;
}

function setStatus(message, isError = false) {
  elements.status.textContent = message;
  elements.status.classList.toggle("error", isError);
}
```

- [ ] **Step 3: Update popup.css**

Replace the full contents of `apps/chrome-extension/popup/popup.css`:

```css
:root {
  color-scheme: light;
  font-family: system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif;
}

body {
  margin: 0;
  min-width: 340px;
  color: #182230;
  background: #f6f8fb;
}

.popup {
  display: grid;
  gap: 12px;
  padding: 16px;
}

.header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
}

h1 {
  margin: 0;
  font-size: 18px;
  font-weight: 650;
}

a {
  color: #1b5fc1;
  font-size: 13px;
  text-decoration: none;
}

.subtitle-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 6px 0;
  border-top: 1px solid #e2e8f0;
  border-bottom: 1px solid #e2e8f0;
}

.subtitle-label {
  font-size: 13px;
  font-weight: 600;
}

.toggle-btn {
  padding: 4px 14px;
  background: #94a3b8;
  border-radius: 20px;
  font-size: 12px;
  font-weight: 650;
  min-width: 48px;
  color: #ffffff;
}

.toggle-btn.is-on {
  background: #2357a5;
}

label {
  display: grid;
  gap: 6px;
  font-size: 13px;
  font-weight: 600;
}

input,
textarea {
  box-sizing: border-box;
  width: 100%;
  border: 1px solid #c8d2df;
  border-radius: 6px;
  padding: 8px 10px;
  color: #182230;
  background: #ffffff;
  font: inherit;
  font-weight: 400;
}

textarea {
  resize: vertical;
}

.actions {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 8px;
}

button {
  border: 0;
  border-radius: 6px;
  padding: 9px 12px;
  color: #ffffff;
  background: #2357a5;
  font: inherit;
  font-weight: 650;
  cursor: pointer;
}

button:disabled {
  cursor: progress;
  opacity: 0.65;
}

.fields {
  display: grid;
  gap: 10px;
}

.status {
  min-height: 18px;
  margin: 0;
  color: #486177;
  font-size: 13px;
}

.status.error {
  color: #b42318;
}
```

- [ ] **Step 4: Verify syntax**

```bash
node --check apps/chrome-extension/popup/popup.js && echo "OK"
```

Expected: `OK`

- [ ] **Step 5: Commit**

```bash
git add apps/chrome-extension/popup/popup.html apps/chrome-extension/popup/popup.js apps/chrome-extension/popup/popup.css
git commit -m "feat(extension): add subtitle clicker toggle to popup"
```

---

### Task 5: Add custom providers section to Options page

**Files:**
- Modify: `apps/chrome-extension/options/options.html`
- Modify: `apps/chrome-extension/options/options.js`
- Modify: `apps/chrome-extension/options/options.css`

Add a "Subtitle Providers" section below the existing settings form. Users enter a domain (e.g. `coursera.org`) and CSS selector (e.g. `.rc-SubtitleViewer`). Entries are stored as `Array<{ hostname, selector }>` in `chrome.storage.local` under key `customProviders`.

- [ ] **Step 1: Update options.html**

Replace the full contents of `apps/chrome-extension/options/options.html`:

```html
<!doctype html>
<html lang="en">
  <head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <title>Language Cards Options</title>
    <link rel="stylesheet" href="options.css">
  </head>
  <body>
    <main class="page">
      <h1>Language Cards Options</h1>

      <form id="settings" class="form">
        <label>
          API base URL
          <input id="apiBaseUrl" name="apiBaseUrl" type="url" required>
        </label>

        <label>
          User ID
          <input id="userId" name="userId" type="number" min="1" step="1" required>
        </label>

        <button type="submit">Save</button>
      </form>

      <p id="status" class="status" role="status"></p>

      <section class="providers-section">
        <h2>Subtitle Providers</h2>
        <p class="providers-hint">
          Add a custom CSS selector for subtitle text on any site.
          Open DevTools on the target page, inspect the subtitle element, and copy its selector.
        </p>

        <table class="providers-table">
          <thead>
            <tr>
              <th>Domain</th>
              <th>CSS Selector</th>
              <th></th>
            </tr>
          </thead>
          <tbody id="providersBody"></tbody>
        </table>

        <div class="add-provider">
          <input id="providerHostname" type="text" placeholder="e.g. coursera.org" autocomplete="off">
          <input id="providerSelector" type="text" placeholder="e.g. .rc-SubtitleViewer" autocomplete="off">
          <button id="addProvider" type="button">Add</button>
        </div>

        <p id="providersStatus" class="status" role="status"></p>
      </section>
    </main>

    <script type="module" src="options.js"></script>
  </body>
</html>
```

- [ ] **Step 2: Update options.js**

Replace the full contents of `apps/chrome-extension/options/options.js`:

```js
import { getSettings, saveSettings, getCustomProviders, saveCustomProviders } from "../shared/config.js";

// --- Settings form ---

const form = document.querySelector("#settings");
const apiBaseUrl = document.querySelector("#apiBaseUrl");
const userId = document.querySelector("#userId");
const status = document.querySelector("#status");

loadSettings();
form.addEventListener("submit", handleSubmit);

async function loadSettings() {
  const settings = await getSettings();
  apiBaseUrl.value = settings.apiBaseUrl;
  userId.value = settings.userId;
}

async function handleSubmit(event) {
  event.preventDefault();

  const settings = await saveSettings({
    apiBaseUrl: apiBaseUrl.value,
    userId: userId.value
  });

  apiBaseUrl.value = settings.apiBaseUrl;
  userId.value = settings.userId;
  status.textContent = "Saved.";
}

// --- Custom providers ---

const providersBody = document.querySelector("#providersBody");
const providerHostname = document.querySelector("#providerHostname");
const providerSelector = document.querySelector("#providerSelector");
const addProviderBtn = document.querySelector("#addProvider");
const providersStatus = document.querySelector("#providersStatus");

loadProviders();
addProviderBtn.addEventListener("click", handleAddProvider);

async function loadProviders() {
  const providers = await getCustomProviders();
  renderProviders(providers);
}

function renderProviders(providers) {
  if (providers.length === 0) {
    providersBody.innerHTML = '<tr><td colspan="3" class="providers-empty">No custom providers yet.</td></tr>';
    return;
  }

  providersBody.innerHTML = "";
  providers.forEach((p, i) => {
    const row = document.createElement("tr");
    row.innerHTML = `
      <td>${escHtml(p.hostname)}</td>
      <td><code>${escHtml(p.selector)}</code></td>
      <td><button type="button" class="delete-btn" data-index="${i}">Remove</button></td>
    `;
    row.querySelector(".delete-btn").addEventListener("click", () => handleDeleteProvider(i));
    providersBody.appendChild(row);
  });
}

async function handleAddProvider() {
  const hostname = providerHostname.value.trim()
    .replace(/^https?:\/\//, "")
    .replace(/\/.*$/, "");
  const selector = providerSelector.value.trim();

  if (!hostname || !selector) {
    providersStatus.textContent = "Both domain and selector are required.";
    return;
  }

  const providers = await getCustomProviders();
  const existing = providers.find(p => p.hostname === hostname);
  if (existing) {
    existing.selector = selector;
  } else {
    providers.push({ hostname, selector });
  }

  await saveCustomProviders(providers);
  renderProviders(providers);
  providerHostname.value = "";
  providerSelector.value = "";
  providersStatus.textContent = "Saved.";
  setTimeout(() => { providersStatus.textContent = ""; }, 2000);
}

async function handleDeleteProvider(index) {
  const providers = await getCustomProviders();
  providers.splice(index, 1);
  await saveCustomProviders(providers);
  renderProviders(providers);
}

function escHtml(str) {
  return String(str)
    .replace(/&/g, "&amp;")
    .replace(/</g, "&lt;")
    .replace(/>/g, "&gt;");
}
```

- [ ] **Step 3: Update options.css**

Replace the full contents of `apps/chrome-extension/options/options.css`:

```css
:root {
  color-scheme: light;
  font-family: system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif;
}

body {
  margin: 0;
  color: #182230;
  background: #f6f8fb;
}

.page {
  width: min(560px, calc(100% - 32px));
  margin: 48px auto;
}

h1 {
  margin: 0 0 20px;
  font-size: 24px;
}

h2 {
  margin: 0 0 8px;
  font-size: 18px;
}

.form {
  display: grid;
  gap: 14px;
}

label {
  display: grid;
  gap: 6px;
  font-size: 14px;
  font-weight: 650;
}

input {
  border: 1px solid #c8d2df;
  border-radius: 6px;
  padding: 9px 11px;
  color: #182230;
  background: #ffffff;
  font: inherit;
  font-weight: 400;
}

button {
  justify-self: start;
  border: 0;
  border-radius: 6px;
  padding: 10px 14px;
  color: #ffffff;
  background: #2357a5;
  font: inherit;
  font-weight: 650;
  cursor: pointer;
}

.status {
  min-height: 20px;
  color: #486177;
}

.providers-section {
  margin-top: 40px;
  padding-top: 24px;
  border-top: 1px solid #e2e8f0;
}

.providers-hint {
  margin: 0 0 16px;
  color: #486177;
  font-size: 14px;
}

.providers-table {
  width: 100%;
  border-collapse: collapse;
  margin-bottom: 16px;
  font-size: 14px;
}

.providers-table th {
  text-align: left;
  padding: 6px 8px;
  border-bottom: 2px solid #c8d2df;
  font-weight: 650;
}

.providers-table td {
  padding: 8px;
  border-bottom: 1px solid #e2e8f0;
  vertical-align: middle;
}

.providers-empty {
  color: #486177;
  font-style: italic;
}

code {
  font-family: ui-monospace, "Cascadia Code", monospace;
  font-size: 13px;
  background: #eef2f7;
  padding: 2px 5px;
  border-radius: 3px;
}

.delete-btn {
  padding: 4px 10px;
  font-size: 13px;
  background: #dc2626;
}

.add-provider {
  display: flex;
  gap: 8px;
  align-items: center;
  flex-wrap: wrap;
}

.add-provider input {
  flex: 1;
  min-width: 140px;
}

.add-provider button {
  justify-self: auto;
  white-space: nowrap;
}
```

- [ ] **Step 4: Verify syntax**

```bash
node --check apps/chrome-extension/options/options.js && echo "OK"
```

Expected: `OK`

- [ ] **Step 5: Commit**

```bash
git add apps/chrome-extension/options/options.html apps/chrome-extension/options/options.js apps/chrome-extension/options/options.css
git commit -m "feat(extension): add custom subtitle providers section to options page"
```

---

### Task 6: Create content/tooltip.css

**Files:**
- Create: `apps/chrome-extension/content/tooltip.css`

This stylesheet is injected alongside `subtitle-clicker.js`. It styles the tooltip overlay and the clickable word spans inside subtitles. Z-index values are near `2147483647` (CSS max) to render above any player UI.

- [ ] **Step 1: Create tooltip.css**

Create `apps/chrome-extension/content/tooltip.css` with the following content:

```css
/* Clickable word spans injected into subtitle containers */
[data-lc-word] {
  cursor: pointer;
  border-radius: 2px;
  transition: background 0.1s;
}

[data-lc-word]:hover {
  background: rgba(255, 255, 255, 0.25);
}

/* Tooltip */
#lc-tooltip {
  position: fixed;
  z-index: 2147483647;
  background: #182230;
  color: #ffffff;
  border-radius: 8px;
  padding: 12px 14px;
  min-width: 160px;
  max-width: 280px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.5);
  font-family: system-ui, -apple-system, sans-serif;
  font-size: 14px;
  line-height: 1.4;
}

.lc-word {
  font-size: 16px;
  font-weight: 650;
  margin-bottom: 6px;
}

.lc-transcription {
  color: #9aafc4;
  font-size: 13px;
  margin-bottom: 2px;
}

.lc-translation {
  margin-bottom: 4px;
}

.lc-loading {
  color: #9aafc4;
}

.lc-actions {
  display: flex;
  gap: 8px;
  margin-top: 10px;
  align-items: center;
}

.lc-btn {
  border: 0;
  border-radius: 5px;
  padding: 6px 10px;
  font-family: inherit;
  font-size: 13px;
  font-weight: 600;
  cursor: pointer;
  color: #ffffff;
}

.lc-btn-translate {
  background: #2357a5;
}

.lc-btn-save {
  background: #12b76a;
}

.lc-btn:disabled {
  opacity: 0.65;
  cursor: progress;
}

.lc-saved {
  color: #12b76a;
  font-weight: 600;
  font-size: 13px;
}

/* Custom overlay for TextTrack mode (sites with native <track> elements) */
#lc-texttrack-overlay {
  position: fixed;
  bottom: 60px;
  left: 50%;
  transform: translateX(-50%);
  z-index: 2147483646;
  text-align: center;
  pointer-events: auto;
  font-family: system-ui, sans-serif;
  font-size: 18px;
  font-weight: 600;
  color: #ffffff;
  text-shadow: 0 1px 4px rgba(0, 0, 0, 0.9);
  background: rgba(0, 0, 0, 0.55);
  padding: 4px 14px;
  border-radius: 4px;
  max-width: 80vw;
}
```

- [ ] **Step 2: Commit**

```bash
git add apps/chrome-extension/content/tooltip.css
git commit -m "feat(extension): add tooltip.css for subtitle clicker"
```

---

### Task 7: Create content/subtitle-clicker.js

**Files:**
- Create: `apps/chrome-extension/content/subtitle-clicker.js`

This is the main content script. It runs in an IIFE with a `window.__lcSubtitleClicker` guard to prevent double-initialization when `executeScript` injects it on a page where the persistent registration also fired.

**Provider resolution order:**
1. `<video>` already has `textTracks` with entries → TextTrack mode (creates a fixed overlay, hides native captions)
2. User-defined provider matching `location.hostname` → mutation mode with custom selector
3. Built-in provider matching `location.hostname` → mutation mode with known selector
4. No match → exits silently

**Word wrapping:** replaces `innerHTML` of the subtitle container with `<span data-lc-word="word">word</span>` elements separated by spaces. `lastWrappedText` prevents the MutationObserver from looping on its own changes.

**Tooltip flow:** click → pause video → show tooltip (word + Translate button) → click Translate → show transcription + translation + Save button → click Save → send `cards.add` message with `example = lastWrappedText` → show "Сохранено ✓" for 1 second → close tooltip → resume video.

- [ ] **Step 1: Create subtitle-clicker.js**

Create `apps/chrome-extension/content/subtitle-clicker.js` with the following content:

```js
(function () {
  if (window.__lcSubtitleClicker) return;
  window.__lcSubtitleClicker = true;

  const BUILT_IN_PROVIDERS = [
    { hostname: "kino.pub", selector: "media-captions", observeType: "mutation" },
    { hostname: "www.youtube.com", selector: ".ytp-caption-segment", observeType: "mutation" },
    { hostname: "www.netflix.com", selector: ".player-timedtext-text-container", observeType: "mutation" }
  ];

  const LC_TOOLTIP_ID = "lc-tooltip";
  let lastWrappedText = "";
  let tooltipCleanup = null;

  init();

  async function init() {
    const provider = await resolveProvider();
    if (!provider) return;
    if (provider.observeType === "texttrack") {
      startTextTrackMode(provider.video);
    } else {
      startMutationMode(provider.selector);
    }
  }

  async function resolveProvider() {
    const video = document.querySelector("video");
    if (video && video.textTracks.length > 0) {
      return { observeType: "texttrack", video };
    }
    const customProviders = await getStoredCustomProviders();
    const custom = customProviders.find(p => p.hostname === location.hostname);
    if (custom) return { observeType: "mutation", selector: custom.selector };
    const builtin = BUILT_IN_PROVIDERS.find(p => p.hostname === location.hostname);
    return builtin || null;
  }

  function getStoredCustomProviders() {
    return new Promise(resolve =>
      chrome.storage.local.get({ customProviders: [] }, r => resolve(r.customProviders))
    );
  }

  // --- Mutation mode ---

  function startMutationMode(selector) {
    const existing = document.querySelector(selector);
    if (existing) { attachToContainer(existing); return; }
    const bodyObs = new MutationObserver(() => {
      const el = document.querySelector(selector);
      if (el) { bodyObs.disconnect(); attachToContainer(el); }
    });
    bodyObs.observe(document.body, { childList: true, subtree: true });
  }

  function attachToContainer(el) {
    processSubtitleEl(el);
    const obs = new MutationObserver(() => processSubtitleEl(el));
    obs.observe(el, { childList: true, subtree: true, characterData: true });
  }

  function processSubtitleEl(el) {
    const text = el.textContent.trim();
    if (!text || text === lastWrappedText) return;
    lastWrappedText = text;
    wrapWords(el, text);
  }

  // --- TextTrack mode ---

  function startTextTrackMode(video) {
    const overlay = getOrCreateOverlay();
    function attachTrack(track) {
      track.mode = "hidden";
      track.addEventListener("cuechange", () => {
        const cue = track.activeCues?.[0];
        if (!cue) { overlay.innerHTML = ""; lastWrappedText = ""; return; }
        const text = (cue.text || "").replace(/<[^>]+>/g, "").trim();
        if (!text || text === lastWrappedText) return;
        lastWrappedText = text;
        wrapWords(overlay, text);
      });
    }
    for (const track of video.textTracks) attachTrack(track);
    video.textTracks.addEventListener("addtrack", e => attachTrack(e.track));
  }

  function getOrCreateOverlay() {
    const existing = document.getElementById("lc-texttrack-overlay");
    if (existing) return existing;
    const el = document.createElement("div");
    el.id = "lc-texttrack-overlay";
    el.className = "lc-texttrack-overlay";
    document.body.appendChild(el);
    return el;
  }

  // --- Word wrapping ---

  function wrapWords(container, text) {
    const words = text.split(/\s+/).filter(Boolean);
    container.innerHTML = words
      .map(w => `<span data-lc-word="${escAttr(w)}">${escHtml(w)}</span>`)
      .join(" ");
    container.querySelectorAll("[data-lc-word]").forEach(span => {
      span.addEventListener("click", e => {
        e.stopPropagation();
        pauseVideo();
        showTooltip(span.getAttribute("data-lc-word"), span);
      });
    });
  }

  // --- Tooltip ---

  function showTooltip(word, anchorEl) {
    removeTooltip();

    const tooltip = document.createElement("div");
    tooltip.id = LC_TOOLTIP_ID;
    tooltip.innerHTML = `
      <div class="lc-word">${escHtml(word)}</div>
      <div class="lc-area"></div>
      <div class="lc-actions">
        <button class="lc-btn lc-btn-translate">Перевести</button>
      </div>`;
    document.body.appendChild(tooltip);
    positionTooltip(tooltip, anchorEl);

    let translation = null;

    tooltip.querySelector(".lc-btn-translate").addEventListener("click", async () => {
      const area = tooltip.querySelector(".lc-area");
      const translateBtn = tooltip.querySelector(".lc-btn-translate");
      translateBtn.disabled = true;
      area.innerHTML = '<span class="lc-loading">···</span>';

      try {
        const data = await sendMsg({ type: "cards.translate", term: word });
        translation = data.result;
        area.innerHTML = [
          translation.transcription
            ? `<div class="lc-transcription">${escHtml(translation.transcription)}</div>`
            : "",
          translation.translation
            ? `<div class="lc-translation">${escHtml(translation.translation)}</div>`
            : ""
        ].join("");

        const saveBtn = document.createElement("button");
        saveBtn.className = "lc-btn lc-btn-save";
        saveBtn.textContent = "Сохранить";
        tooltip.querySelector(".lc-actions").appendChild(saveBtn);

        saveBtn.addEventListener("click", async () => {
          saveBtn.disabled = true;
          try {
            await sendMsg({
              type: "cards.add",
              card: {
                term: word,
                translation: translation.translation || "",
                transcription: translation.transcription || "",
                example: lastWrappedText || null
              }
            });
            tooltip.querySelector(".lc-actions").innerHTML =
              '<span class="lc-saved">Сохранено ✓</span>';
            setTimeout(() => { removeTooltip(); resumeVideo(); }, 1000);
          } catch (err) {
            saveBtn.disabled = false;
            area.textContent = err.message;
          }
        });
      } catch (err) {
        translateBtn.disabled = false;
        area.textContent = err.message;
      }
    });

    const onOutside = e => {
      if (!tooltip.contains(e.target)) { removeTooltip(); resumeVideo(); }
    };
    const onEscape = e => {
      if (e.key === "Escape") { removeTooltip(); resumeVideo(); }
    };
    setTimeout(() => document.addEventListener("click", onOutside, true), 0);
    document.addEventListener("keydown", onEscape);
    tooltipCleanup = () => {
      document.removeEventListener("click", onOutside, true);
      document.removeEventListener("keydown", onEscape);
    };
  }

  function positionTooltip(tooltip, anchorEl) {
    tooltip.style.cssText = "position:fixed;visibility:hidden;top:0;left:0;";
    const tr = tooltip.getBoundingClientRect();
    const ar = anchorEl.getBoundingClientRect();
    let top = ar.top - tr.height - 8;
    if (top < 8) top = ar.bottom + 8;
    let left = ar.left + ar.width / 2 - tr.width / 2;
    left = Math.max(8, Math.min(left, window.innerWidth - tr.width - 8));
    tooltip.style.cssText = `position:fixed;top:${top}px;left:${left}px;`;
  }

  function removeTooltip() {
    if (tooltipCleanup) { tooltipCleanup(); tooltipCleanup = null; }
    document.getElementById(LC_TOOLTIP_ID)?.remove();
  }

  // --- Video control ---

  function pauseVideo() {
    const v = document.querySelector("video");
    if (v && !v.paused) v.pause();
  }

  function resumeVideo() {
    const v = document.querySelector("video");
    if (v?.paused) v.play().catch(() => {});
  }

  // --- Message passing ---

  function sendMsg(message) {
    return new Promise((resolve, reject) => {
      chrome.runtime.sendMessage(message, response => {
        if (chrome.runtime.lastError) {
          reject(new Error(chrome.runtime.lastError.message));
          return;
        }
        if (!response?.ok) {
          reject(new Error(response?.error ?? "Extension request failed."));
          return;
        }
        resolve(response.data);
      });
    });
  }

  // --- Utilities ---

  function escHtml(s) {
    return String(s)
      .replace(/&/g, "&amp;")
      .replace(/</g, "&lt;")
      .replace(/>/g, "&gt;")
      .replace(/"/g, "&quot;");
  }

  function escAttr(s) {
    return String(s).replace(/"/g, "&quot;");
  }
})();
```

- [ ] **Step 2: Verify syntax**

```bash
node --check apps/chrome-extension/content/subtitle-clicker.js && echo "OK"
```

Expected: `OK`

- [ ] **Step 3: Commit**

```bash
git add apps/chrome-extension/content/subtitle-clicker.js
git commit -m "feat(extension): add subtitle-clicker content script"
```

---

### Task 8: Manual end-to-end verification

No automated tests exist for browser content scripts. Verification is done in Chrome.

- [ ] **Step 1: Reload the extension**

1. Open `chrome://extensions`
2. Find "Language Cards Companion"
3. Click Reload (↺)

- [ ] **Step 2: Verify popup toggle on kino.pub**

1. Open any kino.pub episode page
2. Click the extension icon
3. Confirm the popup shows "Subtitle clicker" toggle in Off state
4. Click the toggle → it should change to On (blue)
5. Close and reopen popup → toggle should still show On (state persisted)

- [ ] **Step 3: Verify word wrapping**

1. Start the video, enable subtitles in player settings
2. Right-click inside the subtitles area → Inspect
3. Confirm subtitle words are wrapped in `<span data-lc-word="...">` elements

- [ ] **Step 4: Verify click → translate → save flow**

1. Click a subtitle word → video should pause → tooltip appears with the word and [Перевести] button
2. Click [Перевести] → "···" appears briefly → transcription + translation appear + [Сохранить] button
3. Click [Сохранить] → "Сохранено ✓" appears → after 1 second tooltip closes and video resumes
4. Verify the card was created: send `/list` or check via the Telegram bot

- [ ] **Step 5: Verify click outside / Escape dismissal**

1. Click a word → tooltip appears → click outside the tooltip → tooltip closes, video resumes
2. Click a word → press Escape → tooltip closes, video resumes

- [ ] **Step 6: Verify Options custom provider**

1. Open extension Options page
2. Scroll to "Subtitle Providers" section
3. Add a test entry: domain `test.example.com`, selector `.subtitles`
4. Confirm it appears in the table
5. Click Remove → row disappears
