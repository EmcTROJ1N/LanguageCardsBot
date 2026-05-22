# Subtitle Logger Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a content script to the Chrome extension that automatically monitors subtitle text on kino.pub and outputs it to the browser console using three parallel strategies.

**Architecture:** A single content script (`content/subtitle-logger.js`) is injected on all kino.pub pages at `document_idle`. It starts three independent subtitle-reading strategies simultaneously — MutationObserver, polling, and TextTrack API — each logging to console with a distinct prefix. All strategies deduplicate: they only log when the text changes.

**Tech Stack:** Vanilla JS (no build step), Chrome Extensions Manifest V3, Vidstack custom elements (`<media-player>`, `<media-captions>`).

---

### Task 1: Update manifest.json

**Files:**
- Modify: `apps/chrome-extension/manifest.json`

- [ ] **Step 1: Add content_scripts and host_permissions entry**

Open `apps/chrome-extension/manifest.json`. Replace the existing content with:

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
  "content_scripts": [
    {
      "matches": ["*://kino.pub/*"],
      "js": ["content/subtitle-logger.js"],
      "run_at": "document_idle"
    }
  ],
  "permissions": [
    "storage"
  ],
  "host_permissions": [
    "http://localhost/*",
    "https://localhost/*",
    "http://127.0.0.1/*",
    "https://127.0.0.1/*",
    "*://kino.pub/*"
  ]
}
```

- [ ] **Step 2: Validate manifest JSON**

```bash
node -e "JSON.parse(require('fs').readFileSync('apps/chrome-extension/manifest.json', 'utf8')); console.log('OK')"
```

Expected output: `OK`

- [ ] **Step 3: Commit**

```bash
git add apps/chrome-extension/manifest.json
git commit -m "feat(extension): register subtitle-logger content script for kino.pub"
```

---

### Task 2: Create content/subtitle-logger.js

**Files:**
- Create: `apps/chrome-extension/content/subtitle-logger.js`

- [ ] **Step 1: Create the directory**

```bash
mkdir -p apps/chrome-extension/content
```

- [ ] **Step 2: Create the file with all three strategies**

Create `apps/chrome-extension/content/subtitle-logger.js` with this content:

```javascript
// Strategy A: MutationObserver on <media-captions>
// Works with Vidstack — subtitles are rendered as HTML inside this custom element.
function startMutationObserverStrategy() {
  let lastText = "";

  function observeCaptions(captionsEl) {
    const observer = new MutationObserver(() => {
      const text = captionsEl.textContent.trim();
      if (text && text !== lastText) {
        lastText = text;
        console.log("[MutationObserver]", text);
      }
    });
    observer.observe(captionsEl, { childList: true, subtree: true, characterData: true });
  }

  const existing = document.querySelector("media-captions");
  if (existing) {
    observeCaptions(existing);
    return;
  }

  const bodyObserver = new MutationObserver(() => {
    const el = document.querySelector("media-captions");
    if (el) {
      bodyObserver.disconnect();
      observeCaptions(el);
    }
  });
  bodyObserver.observe(document.body, { childList: true, subtree: true });
}

// Strategy B: Polling every 300ms
// Simple interval that reads textContent of <media-captions> on each tick.
function startPollingStrategy() {
  let lastText = "";

  setInterval(() => {
    const el = document.querySelector("media-captions");
    if (!el) return;
    const text = el.textContent.trim();
    if (text && text !== lastText) {
      lastText = text;
      console.log("[Polling]", text);
    }
  }, 300);
}

// Strategy C: HTML5 TextTrack API
// Standard Web API — works on sites using native <track> elements.
// On kino.pub, Vidstack renders subtitles as HTML overlay (not via native <track>),
// so this strategy may produce no output — that outcome is expected and informative.
function startTextTrackStrategy() {
  function attachToVideo(video) {
    video.textTracks.addEventListener("addtrack", (event) => {
      const track = event.track;
      let lastText = "";
      track.addEventListener("cuechange", () => {
        const cue = track.activeCues?.[0];
        if (!cue) return;
        const text = (cue.text || "").trim();
        if (text && text !== lastText) {
          lastText = text;
          console.log("[TextTrack]", text);
        }
      });
    });
  }

  const existing = document.querySelector("video");
  if (existing) {
    attachToVideo(existing);
    return;
  }

  const bodyObserver = new MutationObserver(() => {
    const el = document.querySelector("video");
    if (el) {
      bodyObserver.disconnect();
      attachToVideo(el);
    }
  });
  bodyObserver.observe(document.body, { childList: true, subtree: true });
}

startMutationObserverStrategy();
startPollingStrategy();
startTextTrackStrategy();
```

- [ ] **Step 3: Check JS syntax**

```bash
node --check apps/chrome-extension/content/subtitle-logger.js && echo "OK"
```

Expected output: `OK`

- [ ] **Step 4: Commit**

```bash
git add apps/chrome-extension/content/subtitle-logger.js
git commit -m "feat(extension): add subtitle-logger content script with three parallel strategies"
```

---

### Task 3: Manual Verification in Chrome

No automated tests exist for content scripts — verification is done in Chrome DevTools.

- [ ] **Step 1: Reload the extension**

1. Open `chrome://extensions`
2. Find "Language Cards Companion"
3. Click the reload (↺) button

- [ ] **Step 2: Open kino.pub with a video that has subtitles enabled**

Navigate to any episode page on kino.pub. Enable subtitles in player settings if they are off.

- [ ] **Step 3: Open DevTools on the page**

Press `F12` → Console tab. Filter by "MutationObserver", "Polling", or "TextTrack" to isolate output.

- [ ] **Step 4: Confirm expected output**

When a subtitle cue appears on screen, you should see at least one (ideally two) of the following in the console:

```
[MutationObserver] Some subtitle text here
[Polling] Some subtitle text here
```

`[TextTrack]` is expected to be silent on kino.pub (Vidstack uses HTML overlay, not native `<track>`).

- [ ] **Step 5: Record findings**

Note which strategies produced output. This determines which one to keep in the next phase of development.
