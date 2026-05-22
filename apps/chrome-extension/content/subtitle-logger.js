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
