# Subtitle Clicker — Design Spec

**Date:** 2026-05-22  
**Scope:** Chrome extension (`apps/chrome-extension`)

## Goal

Allow the user to click on any word in a video subtitle, see its translation, and save it as a vocabulary card — without leaving the player. The extension must work on kino.pub out of the box and be extensible to any other site via a configurable provider registry.

## Activation Model

The extension is **opt-in per domain**, never automatic on all pages.

- The popup shows a toggle: "Enable on this site".
- First enable on a domain → `chrome.scripting.registerContentScripts()` registers a persistent content script for that domain pattern (`*://kino.pub/*`). Chrome retains this registration across sessions automatically.
- Disable → `chrome.scripting.unregisterContentScripts()` removes the registration for that domain.
- The popup toggle reflects the current state (enabled/disabled) for the active tab's hostname.

This requires adding `"scripting"` and `"tabs"` to `permissions` in `manifest.json`. The existing static `content_scripts` entry (subtitle-logger) is removed.

## Provider Registry

Decides how to read subtitle text on the current page. Evaluated in order at content script load:

### Priority order

1. **TextTrack (universal)** — checks if `<video>` has active `textTracks` with loaded cues. If yes, listens to `cuechange` events. Works on sites using native `<track>` elements.

2. **User-defined providers** — fetched from `chrome.storage.local` key `customProviders`. Each entry: `{ hostname, selector }`. Matched against `location.hostname`.

3. **Built-in providers** — hardcoded map:
   | Hostname | Selector |
   |---|---|
   | `kino.pub` | `media-captions` |
   | `www.youtube.com` | `.ytp-caption-segment` |
   | `www.netflix.com` | `.player-timedtext-text-container` |

4. **No match** — content script silently exits. No DOM modifications.

### Provider interface

```js
{
  name: string,           // display name
  selector: string,       // CSS selector of subtitle container
  observeType: "mutation" | "texttrack"
}
```

For `observeType: "mutation"`: MutationObserver watches the container element for DOM changes, reads `textContent.trim()` on each mutation.

For `observeType: "texttrack"`: listens to `video.textTracks` `addtrack` + `cuechange`, reads `activeCues[0].text`.

Both strategies wait for the target element to appear via a `MutationObserver` on `document.body` if it is not present at script load.

## Word Wrapping

When a new subtitle text is detected:

1. Read the subtitle container's current DOM children.
2. Replace text content with individual `<span data-lc-word>word</span>` elements, one per whitespace-delimited token. Punctuation attached to a word stays with it (e.g., `"Hello,"` → one span).
3. Preserve existing HTML structure inside the container (e.g., italic/bold cue spans from Vidstack) — wrap words within text nodes only, do not rewrap existing elements.
4. When subtitle changes, remove previous spans and re-wrap new text.

## Video Pause / Resume

- On word click: `document.querySelector("video").pause()`
- On tooltip close (save success, outside click, Escape key): `video.play()`

## Tooltip

### States

**State 1 — initial (after word click):**
```
┌─────────────────────────┐
│ emergency               │
│                         │
│         [Перевести]     │
└─────────────────────────┘
```

**State 2 — loading translation:**
```
┌─────────────────────────┐
│ emergency               │
│ ···                     │
│         [Перевести]     │
└─────────────────────────┘
```

**State 3 — translation loaded:**
```
┌─────────────────────────┐
│ emergency               │
│ /ɪˈmɜːdʒənsi/          │
│ чрезвычайная ситуация   │
│                         │
│  [Перевести] [Сохранить]│
└─────────────────────────┘
```

**State 4 — saved (1 second, then close + video.play()):**
```
┌─────────────────────────┐
│ emergency               │
│ /ɪˈmɜːdʒənsi/          │
│ чрезвычайная ситуация   │
│                         │
│        Сохранено ✓      │
└─────────────────────────┘
```

### Positioning

Tooltip appears above the clicked `<span>`. If it would overflow the viewport top, it renders below the span instead. Horizontally clamped to viewport edges.

### Dismiss

- Click outside tooltip → close + `video.play()`
- Press `Escape` → close + `video.play()`
- Only one tooltip visible at a time; clicking a new word closes the previous one first.

## Message Protocol (additions to existing service worker)

### `subtitles.getStatus`

Request:
```json
{ "type": "subtitles.getStatus", "hostname": "kino.pub" }
```
Response: `{ "enabled": true | false }`

### `subtitles.enable`

Request:
```json
{ "type": "subtitles.enable", "hostname": "kino.pub" }
```
Calls `chrome.scripting.registerContentScripts` for `*://<hostname>/*`. Returns `{}` on success.

### `subtitles.disable`

Request:
```json
{ "type": "subtitles.disable", "hostname": "kino.pub" }
```
Calls `chrome.scripting.unregisterContentScripts`. Returns `{}` on success.

## Card Saved from Subtitle

When the user saves a card via the tooltip, the `cards.add` message is sent with:
- `term` — the clicked word
- `translation` — from the translate API response
- `transcription` — from the translate API response
- `example` — the full subtitle line text at the time of the click (the entire `textContent` of the subtitle container, not just the word)

## Options Page Extension

A new section "Кастомные субтитры" added to the existing options page.

UI: a table of `{ hostname, selector }` entries. Below it: two inputs ("Домен", "CSS-селектор") + "Добавить" button. Each row has a "Удалить" button.

Storage key: `customProviders` in `chrome.storage.local`. Format: `Array<{ hostname: string, selector: string }>`.

## Files Changed

| File | Action |
|---|---|
| `manifest.json` | Add `"scripting"`, `"tabs"` to permissions; remove static `content_scripts` entry |
| `content/subtitle-logger.js` | Delete |
| `content/subtitle-clicker.js` | Create — provider detection, word wrapping, tooltip logic |
| `content/tooltip.css` | Create — tooltip styles |
| `popup/popup.html` | Add toggle button |
| `popup/popup.js` | Add toggle state logic, send enable/disable messages |
| `popup/popup.css` | Style the toggle |
| `options/options.html` | Add custom providers section |
| `options/options.js` | Add custom providers CRUD |
| `options/options.css` | Style new section |
| `shared/config.js` | Add `getCustomProviders` / `saveCustomProviders` helpers |
| `background/service-worker.js` | Add `subtitles.getStatus`, `subtitles.enable`, `subtitles.disable` handlers |

## Future Work

- Support multi-word selection (drag to select a phrase).
- Add "Skip" button in tooltip to close without saving and resume video immediately.
- Extend built-in provider list (Coursera, Udemy, Kinopoisk, etc.).
- Add visual indicator on the page when subtitle clicker is active (small badge or border).
