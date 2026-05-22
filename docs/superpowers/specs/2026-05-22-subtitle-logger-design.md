# Subtitle Logger — Design Spec

**Date:** 2026-05-22  
**Scope:** Chrome extension (`apps/chrome-extension`)

## Goal

Read subtitle text from the active media player on kino.pub and output it to the browser console. This is a diagnostic/exploratory step: all three candidate strategies run in parallel so the developer can observe which one successfully captures cues.

## New Files

```
apps/chrome-extension/
  content/
    subtitle-logger.js   ← new content script
  manifest.json          ← modified
```

## Manifest Changes

Add `content_scripts` section:

```json
"content_scripts": [
  {
    "matches": ["*://kino.pub/*"],
    "js": ["content/subtitle-logger.js"],
    "run_at": "document_idle"
  }
]
```

Add `*://kino.pub/*` to `host_permissions`.

`content/subtitle-logger.js` is plain JS (no ES module imports) — content scripts in MV3 require explicit `"type": "module"` in the manifest entry to use ES modules, and for this single-file diagnostic script that is unnecessary complexity.

## Strategy A — MutationObserver on `<media-captions>`

kino.pub uses Vidstack, which renders subtitle cues as HTML inside `<media-captions data-part="captions">`.

1. Attach a `MutationObserver` to `document.body` watching for the `media-captions` element to appear in the DOM (childList + subtree).
2. Once found, attach a second `MutationObserver` to `media-captions` itself (childList + subtree + characterData).
3. On each mutation, read `element.textContent.trim()`. Log to console with prefix `[MutationObserver]` only when the value differs from the previous one.

## Strategy B — Polling

1. `setInterval` every 300 ms.
2. Query `document.querySelector('media-captions')`.
3. Read `textContent.trim()`. Log with prefix `[Polling]` only when value differs from previous.

No waiting needed — the interval simply skips iterations where the element is absent.

## Strategy C — HTML5 TextTrack API

Applies to sites using native `<track>` elements. On kino.pub, Vidstack renders subtitles as HTML overlay, so this strategy may yield no output — that is expected and useful to confirm.

1. Wait for `<video>` to appear in the DOM (MutationObserver on document.body).
2. Listen to `video.textTracks` via the `addtrack` event.
3. On each track, attach a `cuechange` listener.
4. On cue change, read `track.activeCues[0]?.text` (VTTCue) or `.getCueAsHTML()`. Log with prefix `[TextTrack]`.

## Console Output Format

```
[MutationObserver] Hello, world.
[Polling] Hello, world.
[TextTrack] Hello, world.
```

Each strategy deduplicates: logs only when the text differs from the last logged value. Empty strings are not logged.

## Deduplication

Each strategy keeps a module-level `let lastText = ""` variable. Log only when `newText !== lastText && newText !== ""`. Update `lastText` after logging.

## Error Handling

No try/catch needed internally — these are read-only DOM observations with no failure modes that require recovery. If an element is absent the strategy waits or skips.

## Future Work

- Once the best strategy is confirmed, remove the other two.
- Extend `matches` to additional streaming sites as needed, or make the selector list configurable.
- Wire confirmed subtitle text into the popup flow (auto-fill `term` field, etc.).
