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
