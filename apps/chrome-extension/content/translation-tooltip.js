(function () {
  if (window.__lcTranslationTooltip) return;

  const TOOLTIP_ID = "lc-tooltip";
  let cleanup = null;

  window.__lcTranslationTooltip = { show, remove };

  function show({ term, anchorRect, context, onClose }) {
    remove();

    const tooltip = document.createElement("div");
    tooltip.id = TOOLTIP_ID;
    tooltip.innerHTML = `
      <div class="lc-word">${escHtml(term)}</div>
      <div class="lc-area"><span class="lc-loading">···</span></div>
      <div class="lc-actions"></div>`;
    document.body.appendChild(tooltip);
    positionTooltip(tooltip, anchorRect);

    const area = tooltip.querySelector(".lc-area");
    const actions = tooltip.querySelector(".lc-actions");

    (async () => {
      try {
        const data = await sendMsg({ type: "cards.translate", term });
        const translation = data.result;
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
        actions.appendChild(saveBtn);
        positionTooltip(tooltip, anchorRect);

        saveBtn.addEventListener("click", async () => {
          saveBtn.disabled = true;
          try {
            await sendMsg({
              type: "cards.add",
              card: {
                term,
                translation: translation.translation || "",
                transcription: translation.transcription || "",
                example: context || null
              }
            });
            actions.innerHTML = '<span class="lc-saved">Сохранено ✓</span>';
            setTimeout(() => { remove(); onClose?.(); }, 1000);
          } catch (err) {
            saveBtn.disabled = false;
            area.textContent = err.message;
          }
        });
      } catch (err) {
        area.textContent = err.message;
      }
    })();

    const onOutside = e => {
      if (!tooltip.contains(e.target)) { remove(); onClose?.(); }
    };
    const onEscape = e => {
      if (e.key === "Escape") { remove(); onClose?.(); }
    };
    setTimeout(() => document.addEventListener("click", onOutside, true), 0);
    document.addEventListener("keydown", onEscape);
    cleanup = () => {
      document.removeEventListener("click", onOutside, true);
      document.removeEventListener("keydown", onEscape);
    };
  }

  function remove() {
    if (cleanup) { cleanup(); cleanup = null; }
    document.getElementById(TOOLTIP_ID)?.remove();
  }

  function positionTooltip(tooltip, anchorRect) {
    tooltip.style.cssText = "position:fixed;visibility:hidden;top:0;left:0;";
    const tr = tooltip.getBoundingClientRect();
    let top = anchorRect.top - tr.height - 8;
    if (top < 8) top = anchorRect.bottom + 8;
    let left = anchorRect.left + anchorRect.width / 2 - tr.width / 2;
    left = Math.max(8, Math.min(left, window.innerWidth - tr.width - 8));
    tooltip.style.cssText = `position:fixed;top:${top}px;left:${left}px;`;
  }

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

  function escHtml(s) {
    return String(s)
      .replace(/&/g, "&amp;")
      .replace(/</g, "&lt;")
      .replace(/>/g, "&gt;")
      .replace(/"/g, "&quot;");
  }
})();
