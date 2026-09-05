(function () {
  if (window.__lcSubtitleClicker) return;
  window.__lcSubtitleClicker = true;

  const BUILT_IN_PROVIDERS = [
    { hostname: "kino.pub", selector: "media-captions", observeType: "mutation" },
    { hostname: "www.youtube.com", selector: ".ytp-caption-segment", observeType: "mutation" },
    { hostname: "www.netflix.com", selector: ".player-timedtext-text-container", observeType: "mutation" }
  ];

  let lastWrappedText = "";

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
    const text = getSubtitleText(el);
    if (text === lastWrappedText) return;
    lastWrappedText = text;
    const overlay = getOrCreateMutationOverlay();
    if (!text) {
      overlay.style.display = "none";
      overlay.innerHTML = "";
      el.style.visibility = "";
      return;
    }
    el.style.visibility = "hidden";
    overlay.style.display = "";
    wrapWords(overlay, text);
    positionMutationOverlay(overlay, el);
  }

  function positionMutationOverlay(overlay, subtitleEl) {
    const rect = subtitleEl.getBoundingClientRect();
    const oh = overlay.offsetHeight || 36;
    const top = Math.max(rect.top + 4, rect.bottom - oh - 40);
    overlay.style.top = top + "px";
    overlay.style.left = (rect.left + rect.width / 2) + "px";
    overlay.style.transform = "translateX(-50%)";
  }

  function getSubtitleText(el) {
    const parts = [];
    function collectLeaves(node) {
      if (node.nodeType === Node.TEXT_NODE) {
        const t = node.textContent.trim();
        if (t) parts.push(t);
      } else if (node.nodeType === Node.ELEMENT_NODE) {
        const hasElementChild = Array.from(node.childNodes).some(n => n.nodeType === Node.ELEMENT_NODE);
        if (hasElementChild) {
          for (const child of node.childNodes) collectLeaves(child);
        } else {
          const t = node.textContent.trim();
          if (t) parts.push(t);
        }
      }
    }
    for (const child of el.childNodes) collectLeaves(child);
    return parts.join(" ").replace(/\s+/g, " ").trim();
  }

  function getOrCreateMutationOverlay() {
    let overlay = document.getElementById("lc-mutation-overlay");
    if (!overlay) {
      overlay = document.createElement("div");
      overlay.id = "lc-mutation-overlay";
      overlay.style.display = "none";
      document.body.appendChild(overlay);
    }
    return overlay;
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
        window.__lcTranslationTooltip?.show({
          term: span.getAttribute("data-lc-word"),
          anchorRect: span.getBoundingClientRect(),
          context: lastWrappedText,
          onClose: resumeVideo
        });
      });
    });
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
